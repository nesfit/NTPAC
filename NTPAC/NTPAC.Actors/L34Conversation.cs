using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using NTPAC.Common.Models;
using NTPAC.Messages;
using PacketDotNet;

namespace NTPAC.Actors
{
//    public class L34Conversation : ReceiveActor
//    {
//        private readonly Dictionary<L3ConversationKeyClass, IActorRef> _l3Conversations = new Dictionary<L3ConversationKeyClass, IActorRef>();
//
//        private readonly L3ConversationKeyClass _l3Key;
//        private readonly L4ConversationKeyClass _l4Key;
//        private readonly ILoggingAdapter _log = Context.GetLogger();
//
//        public L34Conversation(L3ConversationKeyClass l3Key, L4ConversationKeyClass l4Key)
//        {
//            this._l3Key = l3Key;
//            this._l4Key = l4Key;
//            this.Become(this.AnalysisBehaviour);
//        }
//
//        public static Props Props(L3ConversationKeyClass l3Key, L4ConversationKeyClass l4Key) => Akka.Actor.Props.Create(() => new L34Conversation(l3Key, l4Key));
//
//        private void AnalysisBehaviour()
//        {
//            this.Receive<ProcessPacketRequest>(msg => this.OnProcessPacket(msg));
//            this.Receive<ProcessRawPacketRequest>(msg => this.OnProcessRawPacket(msg));
//        }
//
//        private void OnProcessPacket(ProcessPacketRequest msg)
//        {
//            this._log.Info($"L34C OnProcessPacket");
//
//            var       l3ConversationKey = this._l3Key;
//            IActorRef l3ConversationActor;
//            if (!this._l3Conversations.ContainsKey(this._l3Key))
//            {
//                this._log.Info($"Creating new L3C actor: {l3ConversationKey}");
//                l3ConversationActor = Context.ActorOf(L3Conversation.Props(l3ConversationKey) /*, l3ConversationKey.ToString() */);
//                this._l3Conversations.Add(l3ConversationKey, l3ConversationActor);
//            }
//            else
//            {
//                l3ConversationActor = this._l3Conversations[l3ConversationKey];
//            }
//
//            l3ConversationActor.Forward(msg);
//        }
//
//        private void OnProcessRawPacket(ProcessRawPacketRequest msg)
//        {
//            if (msg.RawPacketData == null)
//            {
//                throw new Exception("Raw packet with no payload data");
//            }
//
//            var processTask = Task.Run(() =>
//            {
//                var parsedPacket = Packet.ParsePacket(msg.LinkType, msg.RawPacketData);
//                return new ProcessPacketRequest(msg.DateTime, parsedPacket);
//            });
//            // Pipe the task's result to self, but use this ProcessRawPacketRequest message sender as a sender of the result message
//            processTask.PipeTo(this.Self, this.Sender);
//        }
//    }
}
