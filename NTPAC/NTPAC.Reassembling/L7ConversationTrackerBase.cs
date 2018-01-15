using System.Net;
using NTPAC.Common.Models;
using NTPAC.Reassembling.Enums;
using NTPAC.Reassembling.Models;
using PacketDotNet.IP;

namespace NTPAC.Reassembling
{
    public abstract class L7ConversationTrackerBase
    {
        public L7ConversationTrackerBase(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint)
        {
            this.SourceEndPoint = sourceEndPoint;
            this.DestinationEndPoint = destinationEndPoint;
        }

        public readonly IPEndPoint SourceEndPoint;
        public readonly IPEndPoint DestinationEndPoint;
        public abstract IPProtocolType ProtocolType();
        
        public abstract L7Conversation ProcessFrame(Frame frame);
        public abstract L7Conversation CloseCurrentSession();

        protected FlowDirection GetFrameFlowDirection(Frame frame)
        { 
            if (frame.IpProtocol != this.ProtocolType()) { return FlowDirection.None; }

            if (frame.SourceAddress.Equals(this.SourceEndPoint.Address) && frame.SourcePort == this.SourceEndPoint.Port
                && frame.DestinationAddress.Equals(this.DestinationEndPoint.Address) && frame.DestinationPort == this.DestinationEndPoint.Port) { return FlowDirection.Up; }
            if (frame.SourceAddress.Equals(this.DestinationEndPoint.Address) && frame.SourcePort == this.DestinationEndPoint.Port
                && frame.DestinationAddress.Equals(this.SourceEndPoint.Address) && frame.DestinationPort == this.SourceEndPoint.Port) { return FlowDirection.Down; }
            
            return FlowDirection.None;
        }

        protected L7Conversation CreateL7Conversation(L7Flow upFlow, L7Flow downFlow)
            => new L7Conversation(this.SourceEndPoint, this.DestinationEndPoint, this.ProtocolType(), upFlow, downFlow);
    }
}
