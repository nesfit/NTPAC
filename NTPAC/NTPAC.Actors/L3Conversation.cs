using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using NTPAC.Common.Models;
using NTPAC.Messages;

namespace NTPAC.Actors
{
    public class L3Conversation : ReceiveActor
    {
        private readonly Dictionary<L4ConversationKeyClass, IActorRef> _l4Conversations = new Dictionary<L4ConversationKeyClass, IActorRef>();

        private readonly ILoggingAdapter _log = Context.GetLogger();

        private L3ConversationKeyClass _l3Key;

        public L3Conversation(L3ConversationKeyClass l3Key)
        {
            this._l3Key = l3Key;

            this.Become(this.AnalysisBehaviour);
        }

        public static Props Props(L3ConversationKeyClass l3Key) => Akka.Actor.Props.Create(() => new L3Conversation(l3Key));

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
            foreach (var children in Context.GetChildren())
            {
                children.Tell(fin);
            }
            this.Become(this.ClosedBehaviour);
        }
        
        private void OnFrame(Frame frame)
        {
            //this._log.Info("L3C OnProcessPacket");

            var l4Key = frame.L4ConversationKey;
            IActorRef l4ConversationActor;
            if (!this._l4Conversations.ContainsKey(l4Key))
            {
                l4ConversationActor = Context.ActorOf(L4Conversation.Props(l4Key, frame.SourceEndPoint, frame.DestinationEndPoint));
                this._l4Conversations.Add(l4Key, l4ConversationActor);
            }
            else
            {
                l4ConversationActor = this._l4Conversations[l4Key];
            }

            l4ConversationActor.Forward(frame);
        }
    }
}
