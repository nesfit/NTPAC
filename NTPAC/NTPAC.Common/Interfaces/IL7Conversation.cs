using System;
using System.Collections.Generic;
using System.Net;
using NTPAC.ConversationTracking.Models;
using PacketDotNet.IP;

namespace NTPAC.Common.Interfaces
{
  public interface IL7Conversation
  {
    Int32 ConversationSegmentNum { get; }
    IPEndPoint DestinationEndPoint { get; }
    IEnumerable<IL7Pdu> DownPdus { get; }
    Boolean LastConversationSegment { get; }
    IEnumerable<IL7Pdu> Pdus { get; }
    IPProtocolType ProtocolType { get; }
    IPEndPoint SourceEndPoint { get; }
    IEnumerable<IL7Pdu> UpPdus { get; }
  }
}
