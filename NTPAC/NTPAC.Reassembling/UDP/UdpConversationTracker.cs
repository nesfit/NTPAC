using System;
using System.Net;
using NTPAC.Common.Models;
using NTPAC.Reassembling.Enums;
using NTPAC.Reassembling.Models;
using PacketDotNet.IP;

namespace NTPAC.Reassembling.UDP
{
    public class UdpConversationTracker : L7ConversationTrackerBase
    {
        private static readonly TimeSpan UDPSessionAliveTimeout = TimeSpan.FromMinutes(10);

        private L7Flow _upFlow;
        private L7Flow _downFlow;

        public UdpConversationTracker(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint) : base(sourceEndPoint, destinationEndPoint)
        {
            this.ResetFlows();
        }

        public override IPProtocolType ProtocolType() => IPProtocolType.UDP;
        
        public override L7Conversation ProcessFrame(Frame frame)
        {
            var frameDirection = this.GetFrameFlowDirection(frame);
            var flow = this.GetOrCreateFlow(frameDirection);
            L7Conversation newL7Conversation = null;
            if (flow.LastSeen.HasValue && (frame.TimestampTicks - flow.LastSeen.Value) > UDPSessionAliveTimeout.Ticks)
            {
                newL7Conversation = this.CloseCurrentSession();
            }
            flow.AddPdu(new L7PDU(frame, frameDirection));

            return newL7Conversation;
        }
        
        public override L7Conversation CloseCurrentSession()
        {
            L7Conversation newL7Conversation = null;
            if (this._upFlow.Any() || this._downFlow.Any())
            {
                newL7Conversation = this.CreateL7Conversation(this._upFlow, this._downFlow);
                this.ResetFlows();
            }
            return newL7Conversation;
        }

        private L7Flow GetOrCreateFlow(FlowDirection frameDirection)
        {
            switch (frameDirection)
            {
                case FlowDirection.Up:
                    return GetOrCreateFlow(ref this._upFlow);
                case FlowDirection.Down:
                    return GetOrCreateFlow(ref this._downFlow);
                default:
                    throw new ArgumentOutOfRangeException(nameof(frameDirection), frameDirection, null);
            }
        }

        private static L7Flow GetOrCreateFlow(ref L7Flow flow) => flow ?? (flow = new L7Flow());
       
        private void ResetFlows()
        {
            this._upFlow = null;
            this._downFlow = null;
        }
    }
}
