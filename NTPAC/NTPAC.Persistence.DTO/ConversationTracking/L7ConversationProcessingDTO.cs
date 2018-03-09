using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.Persistence.DTO.ConversationTracking
{
  public class L7ConversationProcessingDTO : IL7Conversation
  {
    public PacketDotNet.IPProtocolType ProtocolType { get; set; }
    public IPEndPoint SourceEndPoint { get; set; }
    public IPEndPoint DestinationEndPoint { get; set; }
    
    public IReadOnlyCollection<IL7Pdu> Pdus { get; set; }
    public IEnumerable<IL7Pdu> UpPdus =>
      this.Pdus.Where(pdu => pdu.Direction == NTPAC.ConversationTracking.Interfaces.Enums.FlowDirection.Up);
    public IEnumerable<IL7Pdu> DownPdus =>
      this.Pdus.Where(pdu => pdu.Direction == NTPAC.ConversationTracking.Interfaces.Enums.FlowDirection.Down);

    public Guid Id { get; set; }
  }
}
