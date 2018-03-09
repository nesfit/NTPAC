using System;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;

namespace NTPAC.Persistence.DTO.ConversationTracking
{
  public class L7PduDTO : IL7Pdu
  {
    public FlowDirection Direction { get; set; }
    
    public Int64 FirstSeenTicks { get; set; }
    public Int64 LastSeenTicks { get; set; }
    
    public DateTime FirstSeen => new DateTime(this.FirstSeenTicks);
    public DateTime LastSeen => new DateTime(this.LastSeenTicks);

    public Byte[] Payload { get; set; }
    public Int32 PayloadLen { get; set; }
    
    public Int32 CompareTo(IL7Pdu other) => this.FirstSeenTicks.CompareTo(other.FirstSeenTicks);
  }
}
