using System;
using System.Net;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.Persistence.Entities
{
  public class L7ConversationEntity : IL7ConversationEntity
  {
    public Guid CaptureId { get; set; }
    public IPEndPointEntity DestinationEndPoint { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public Int32 ProtocolType { get; set; }
    public IPEndPointEntity SourceEndPoint { get; set; }
    public Int64 PduCount { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();

    public IPAddress SourceIPAddress => this.SourceEndPoint.Address;
    public Int32 SourcePort => this.SourceEndPoint.Port;
    public IPAddress DestinationIPAddress => this.DestinationEndPoint.Address;
    public Int32 DestinationPort => this.DestinationEndPoint.Port;
  }
}
