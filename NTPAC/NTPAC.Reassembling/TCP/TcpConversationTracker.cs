using System.Net;
using NTPAC.Common.Models;
using NTPAC.Reassembling.Models;
using PacketDotNet.IP;

namespace NTPAC.Reassembling.TCP
{
    public class TcpConversationTracker : L7ConversationTrackerBase
    {
        public TcpConversationTracker(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint) : base(sourceEndPoint, destinationEndPoint)
        {
        }

        public override IPProtocolType ProtocolType() => IPProtocolType.TCP;
        
        public override L7Conversation ProcessFrame(Frame frame) => null;

        public override L7Conversation CloseCurrentSession() => null;
    }
}
