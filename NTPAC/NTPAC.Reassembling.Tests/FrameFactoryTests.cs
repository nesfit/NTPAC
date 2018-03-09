using System.Linq;
using NTPAC.ConversationTracking.Factories;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class FrameFactoryTests : ReassemblingTestBase
  {
    public FrameFactoryTests(ITestOutputHelper output) : base(output)
    {
    }
    
    [Fact]
    public void Frame1()
    {
      var frames = this.GetPacketsFromPcap(TestPcapFile.IsaHttpRetransmission).ToList();
      
      var (packet, timestampTicks) = frames[5];
      var frame = FrameFactory.CreateFromPacket(packet, timestampTicks);
      
      Assert.Equal(60, frame.Data.Length);
      Assert.Equal(0, frame.L7PayloadDataSegmentLength);
      Assert.True(frame.TcpFAck);
      Assert.Equal(3594821724, frame.TcpSequenceNumber);
    }
  }
}
