using System;
using System.Collections.Generic;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL7Conversation
  {
    IPProtocolType ProtocolType { get; }
    IPEndPoint SourceEndPoint { get; }
    IPEndPoint DestinationEndPoint { get; }

    IReadOnlyCollection<IL7Pdu> Pdus { get; }
    IEnumerable<IL7Pdu> UpPdus { get; }
    IEnumerable<IL7Pdu> DownPdus { get; }

    Guid Id { get; }
  }
}
