using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.TCP;
using NTPAC.Reassembling.UDP;
using NTPAC.Tests;
using SharpPcap;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class RawCaptureBuilderTests : ReassemblingTestBase
  {
    public RawCaptureBuilderTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void UdpTest_dns()
    {
      var l7Conversations = this.ReassembleUdpL7Conversations(TestPcapFile.Dns1);
      Assert.Single(l7Conversations);
      var frames = ((L7Conversation)l7Conversations.First()).ReconstructRawCaptures().ToArray();
      Assert.Equal(2, frames.Length);

      var f1 = frames[0];
      Assert.Equal(88, f1.Data.Length);
      Assert.Equal(new PosixTimeval(63520359124, 120427), f1.Timeval);
      var f2 = frames[1];
      Assert.Equal(188, f2.Data.Length);
      Assert.Equal(new PosixTimeval(63520359124, 251658), f2.Timeval);
    }

    [Fact]
    public void UdpTest_dns_fragment()
    {
      var l7Conversations = this.ReassembleUdpL7Conversations(TestPcapFile.IsaHttpFragment);
      Assert.Single(l7Conversations);
      var frames = ((L7Conversation)l7Conversations.First()).ReconstructRawCaptures().ToArray();
      Assert.Equal(3, frames.Length);

      var f1 = frames[0];
      Assert.Equal(88, f1.Data.Length);
      Assert.Equal(new PosixTimeval(63520360197, 508192), f1.Timeval);
      var f2 = frames[1];
      Assert.Equal(1514, f2.Data.Length);
      Assert.Equal(new PosixTimeval(63520360197, 509081), f2.Timeval);
      var f3 = frames[2];
      Assert.Equal(400, f3.Data.Length);
      Assert.Equal(new PosixTimeval(63520360197, 509087), f3.Timeval);
    }

    [Fact]
    public void TcpTest_http()
    {
      var l7Conversations = this.ReassembleTcpL7Conversations(TestPcapFile.HttpTest1);
      Assert.Single(l7Conversations);
      var frames = ((L7Conversation)l7Conversations.First()).ReconstructRawCaptures().ToArray();
      Assert.Equal(9, frames.Length);

      var f1 = frames[0];
      Assert.Equal(62, f1.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 560614), f1.Timeval);
      var f2 = frames[1];
      Assert.Equal(62, f2.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 586851), f2.Timeval);
      var f3 = frames[2];
      Assert.Equal(54, f3.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 587022), f3.Timeval);
      var f4 = frames[3];
      Assert.Equal(182, f4.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 587158), f4.Timeval);
      var f5 = frames[4];
      Assert.Equal(60, f5.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 613459), f5.Timeval);
      var f6 = frames[5];
      Assert.Equal(1481, f6.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 613832), f6.Timeval);
      var f7 = frames[6];
      Assert.Equal(54, f7.Data.Length);
      Assert.Equal(new PosixTimeval(63520359176, 783576), f7.Timeval);
      var f8 = frames[7];
      Assert.Equal(60, f8.Data.Length);
      Assert.Equal(new PosixTimeval(63520359186, 613638), f8.Timeval);
      var f9 = frames[8];
      Assert.Equal(54, f9.Data.Length);
      Assert.Equal(new PosixTimeval(63520359186, 614068), f9.Timeval);
     
    }
    
    private IReadOnlyList<IL7Conversation> ReassembleUdpL7Conversations(String pcapFileName) =>
      this.ReassembleL7Conversations<UdpConversationTracker>(pcapFileName);
    
    private IReadOnlyList<IL7Conversation> ReassembleTcpL7Conversations(String pcapFileName) =>
      this.ReassembleL7Conversations<TcpConversationTracker>(pcapFileName);
  }
}
