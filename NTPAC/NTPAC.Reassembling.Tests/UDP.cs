using System.Linq;
using System.Net;
using NTPAC.Reassembling.UDP;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
    public class UDP : ReassemblingTestBase
    {
        public UDP(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public void Test1()
        {
            // Simple DNS request - response
            var frames = this.GetFramesFromPcap("dns_1.pcapng");

            var originatorFrame = frames.First();
            var udpReassembler = new UdpConversationTracker(originatorFrame.SourceEndPoint, originatorFrame.DestinationEndPoint);
            Assert.Null(udpReassembler.ProcessFrame(frames.ElementAt(0)));
            Assert.Null(udpReassembler.ProcessFrame(frames.ElementAt(1)));
            var l7Conversation = udpReassembler.CloseCurrentSession();
            Assert.NotNull(l7Conversation);
            
            Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.50"), 59416));
            Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("192.168.1.1"), 53));

            var pdus = l7Conversation.PDUs;
            Assert.Equal(pdus.Count(), 2);
        }
    }
}
