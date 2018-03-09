using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.Reassembling.UDP;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class UdpConversationTrackerTests : ReassemblingTestBase
  {
    public UdpConversationTrackerTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void DNS_1_ProcessFrame_AllFramesProcessed()
    {
      // Simple DNS request - response
      var l7Conversations = this.ReassembleL7Conversations(TestPcapFile.Dns1);

      DNS_1_ProcessFrame(l7Conversations);
    }

    [Fact(Skip = "Fails on Windows")]
    public void DNS_1_ProcessFrame_AllFramesProcessed_WindowsFails()
    {
      // Simple DNS request - response
      var l7Conversations = this.ReassembleL7Conversations(TestPcapFile.Dns1);

      DNS_1_ProcessFrame(l7Conversations);
    }

    private static void DNS_1_ProcessFrame(IReadOnlyCollection<IL7Conversation> l7Conversations)
    {
      Assert.NotNull(l7Conversations);
      Assert.Single(l7Conversations);
      var l7Conversation = l7Conversations.First();

      Assert.Equal(l7Conversation.SourceEndPoint, new IPEndPoint(IPAddress.Parse("147.229.176.17"), 60416));
      Assert.Equal(l7Conversation.DestinationEndPoint, new IPEndPoint(IPAddress.Parse("147.229.8.12"), 53));

      var pdus = l7Conversation.Pdus.ToArray();
      Assert.Equal(FlowDirection.Up, pdus[0].Direction);
      Assert.Equal(46, pdus[0].PayloadLen);
      Assert.Equal(FlowDirection.Down, pdus[1].Direction);
      Assert.Equal(146, pdus[1].PayloadLen);
    }
    
    [Fact]
    public void UDP_1()
    {
      var l7Conversations = this.ReassembleL7Conversations(TestPcapFile.IsaHttpDns);
      Assert.Equal(8, l7Conversations.Count);

      var l7Conversation1 = l7Conversations[0];
      var l7Conversation1Up = l7Conversation1.UpPdus.ToArray();
      Assert.Equal(3, l7Conversation1Up.Length);
      var l7Conversation1Pdus = l7Conversation1.UpPdus.ToList();
      Assert.Equal(13, l7Conversation1Pdus[0].Payload.Length);
      Assert.Equal(13, l7Conversation1Pdus[1].Payload.Length);
      Assert.Equal(13, l7Conversation1Pdus[2].Payload.Length);
      
      var l7Conversation8 = l7Conversations[7];
      var l7Conversation8Up = l7Conversation8.UpPdus.ToArray();
      Assert.Single(l7Conversation8Up);
      var l7Conversation8Pdus = l7Conversation8.UpPdus.ToList();
      Assert.Equal(13, l7Conversation8Pdus[0].Payload.Length);

    }

    private IReadOnlyList<IL7Conversation> ReassembleL7Conversations(String pcapFileName) =>
      this.ReassembleL7Conversations<UdpConversationTracker>(pcapFileName);
  }
}
