using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using NTPAC.Common.Models;
using NTPAC.Messages;
using NTPAC.Reassembling;
using PacketDotNet;
using PacketDotNet.IP;

namespace NTPAC.Actors
{
    public class Capture : ReceiveActor
    {
        public static String TypeName = nameof(Capture);
        
        private readonly CaptureInfo _info;
        
        private readonly SortedList<Int64, ProcessRawPacketBatchRequest> _batchReorderBuffer = new SortedList<Int64, ProcessRawPacketBatchRequest>();
        private Int64 _currentBatchSeqId;
        
        private readonly Dictionary<L3ConversationKeyClass, IActorRef> _l3Conversations = new Dictionary<L3ConversationKeyClass, IActorRef>();

        private readonly Ipv4DefragEngine _ipv4DefragEngine = new Ipv4DefragEngine(); 
        
        private readonly ILoggingAdapter _log = Context.GetLogger();


        public Capture(CaptureInfo info)
        {
            this._info = info;

            this._log.Info($"{this.Self} started for {info.Filename}");

            this.Become(this.AnalysisBehaviour);
        }

        public static Props Props(CaptureInfo info) => Akka.Actor.Props.Create(() => new Capture(info));

        private void AnalysisBehaviour()
        {
            this.Receive<ProcessRawPacketBatchRequest>(msg => this.OnProcessRawPacketBatch(msg));
            this.Receive<ProcessRawPacketRequest>(msg => this.OnProcessRawPacket(msg));
            this.Receive<Frame>(msg => this.OnFrame(msg));
            this.Receive<FinalizeProcessingRequest>(msg => this.OnFinalizeProcessing(msg));
        }

        private void ClosedBehaviour()
        {   
        }

        private void OnFinalizeProcessing(FinalizeProcessingRequest fin)
        {
            foreach (var children in Context.GetChildren())
            {
                children.Tell(fin);
            }
            this.Become(this.ClosedBehaviour);
        }

        private void OnFrame(Frame frame) {
            this._log.Info("Capture OnProcessPacket");


            var l3Key = frame.L3ConversationKey;
            IActorRef l3ConversationActor;
            if (!this._l3Conversations.ContainsKey(l3Key))
            {
                //this._log.Debug($"Creating new L3C actor: {l3Key}");
                l3ConversationActor = Context.ActorOf(L3Conversation.Props(l3Key) /*, l3Key.ToString() */);
                this._l3Conversations.Add(l3Key, l3ConversationActor);
            }
            else
            {
                l3ConversationActor = this._l3Conversations[l3Key];
            }

            l3ConversationActor.Tell(frame, this.Self);
        }

        private void OnProcessRawPacket(ProcessRawPacketRequest msg)
        {
            this._log.Info("Capture OnProcessRawPacket");

            if (msg.RawPacketData == null)
            {
                this._log.Info("OnProcessRawPacket no raw packet data");
                return;
            }

            try
            {
                var parsedPacket = Packet.ParsePacket(msg.LinkType, msg.RawPacketData);
                if (!(parsedPacket is IpPacket ipPacket))
                {
                    this._log.Info("Ignoring non-IP packet");
                    return;
                }

                Frame frame;
                // Attempt to defragment if needed
                if (ipPacket is IPv4Packet ipv4Packet && Ipv4DefragEngine.Ipv4PacketIsFragmented(ipv4Packet))
                {
                    var defragResult = this._ipv4DefragEngine.TryFragmentDefrag(new IPFragment(ipv4Packet, msg.RecreateOriginalRawCapture()));
                    if (defragResult == null)
                    {
                        this._log.Debug("Packet fragment stored to defragmentation buffer");
                        return;
                    }
                    var (fragmentedRawCaptures, defragmentedIpv4Packet) = defragResult;
                    if (!fragmentedRawCaptures.Any())
                    {
                        throw new Exception("Defragmented frame has no raw captures");
                    }
                    // TODO frame nums in RawCapture
                    frame = new Frame(defragmentedIpv4Packet, fragmentedRawCaptures.First().Timeval.Date.Ticks);
                }
                else
                {
                    frame = new Frame(parsedPacket, msg.DateTimeTicks);
                }
                
                if (frame.Valid)
                {
                    this.Self.Tell(frame);
                }
               
            }
            catch (Exception ex)
            {
                this._log.Error(ex, "Parsing failed");
            }
        }

        private void OnProcessRawPacketBatch(ProcessRawPacketBatchRequest batch)
        {
            var batchRequests = batch.ProcessRawPacketRequests;
            var batchSeqId    = batch.SeqId;
            Console.WriteLine($"[{this.Self}] OnProcessRawPacketBatch ({batchSeqId}): {batchRequests.Count()} packets");

            Object response = null;
            try
            {
                if (batchSeqId == this._currentBatchSeqId)
                {
                    this.ForwardPacketBatch(batch);
                    if (this._batchReorderBuffer.Any())
                    {
                        this.ReplayPacketBatchReorderBuffer();
                    }
                }
                else if (batchSeqId > this._currentBatchSeqId)
                {
                    this.StorePacketBatchToReorderBuffer(batch);
                }
                else
                {
                    throw new Exception($"Received batch from the past (possible duplicate). Current {this._currentBatchSeqId}, got {batchSeqId}");
                }

                response = "ok";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                response = new Failure {Exception = e};
            }
            finally
            {
                this.Sender.Tell(response, this.Self);
            }
        }

        private void ReplayPacketBatchReorderBuffer()
        {
            var startSeqId = this._currentBatchSeqId;
            // Forward batches as long as they are in correct order
            foreach (var batch in this._batchReorderBuffer.Values)
            {
                if (batch.SeqId != this._currentBatchSeqId)
                {
                    break;
                }

                this.ForwardPacketBatch(batch);
            }

            Console.WriteLine($"[{this.Self}] Replayed {this._currentBatchSeqId - startSeqId} batches");
            // Remove forwarded batches
            for (var seqId = startSeqId; seqId < this._currentBatchSeqId; seqId++)
            {
                this._batchReorderBuffer.Remove(seqId);
            }
        }

        private void ForwardPacketBatch(ProcessRawPacketBatchRequest batch)
        {
            var batchSeqId = batch.SeqId;
            if (batchSeqId != this._currentBatchSeqId)
            {
                throw new Exception($"Batch out of order. Expected {this._currentBatchSeqId}, got {batchSeqId}");
            }

            foreach (var processRawPacketRequest in batch.ProcessRawPacketRequests)
            {
                this.Self.Tell(processRawPacketRequest, this.Self);
            }
            this._currentBatchSeqId = batchSeqId + 1;
        }

        private void StorePacketBatchToReorderBuffer(ProcessRawPacketBatchRequest batch) { this._batchReorderBuffer.Add(batch.SeqId, batch); }
    }
}
