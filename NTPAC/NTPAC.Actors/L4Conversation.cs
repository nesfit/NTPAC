using System;
using System.Net;
using Akka.Actor;
using Akka.Event;
using NTPAC.Common.Models;
using NTPAC.Messages;
using NTPAC.Reassembling;
using NTPAC.Reassembling.Models;

namespace NTPAC.Actors
{
    public class L4Conversation : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly L4ConversationKeyClass _l4Key;
        private readonly L7ConversationTrackerBase _l7ConversationTracker;

        public L4Conversation(L4ConversationKeyClass l4Key, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint)
        {
            this._l4Key = l4Key;
            this._l7ConversationTracker = L7ConversationTrackerFactory.Create(sourceEndPoint, destinationEndPoint, l4Key.GetProtocolType);
            this.Become(this.AnalysisBehaviour);
        }

        public static Props Props(L4ConversationKeyClass l4Key, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint) => Akka.Actor.Props.Create(() => new L4Conversation(l4Key, sourceEndPoint, destinationEndPoint));

        private void AnalysisBehaviour()
        {
            this.Receive<Frame>(msg => this.OnFrame(msg));
            this.Receive<FinalizeProcessingRequest>(msg => this.OnFinalizeProcessing(msg));
        }

        private void ClosedBehaviour()
        {   
        }

        private void OnFinalizeProcessing(FinalizeProcessingRequest fin)
        {
            this._log.Info("L4C OnFinalizeProcessing");
            
            var newL7Conversation = this._l7ConversationTracker.CloseCurrentSession();
            if (newL7Conversation != null)
            {
                this.HandleL7Conversation(newL7Conversation);
            }

            this.Become(this.ClosedBehaviour);
        }

        private void OnFrame(Frame frame)
        {
            this._log.Info("L4C OnProcessPacket");

            var newL7Conversation = this._l7ConversationTracker.ProcessFrame(frame);
            if (newL7Conversation != null)
            {
                this.HandleL7Conversation(newL7Conversation);
            }

            //this.Sender.Tell(new PacketProcessed(), this.Sender);
        }

        private void HandleL7Conversation(L7Conversation l7Conversation)
        {
            //this._log.Info($"L4C HandleNewL7Conversation: {l7Conversation}");
            try
            {
                var c = l7Conversation.ToString();
                this._log.Info($"L4C HandleNewL7Conversation: {c}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }   
        }
    }
}
