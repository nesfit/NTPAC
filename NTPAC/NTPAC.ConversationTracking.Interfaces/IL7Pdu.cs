using System;
using NTPAC.ConversationTracking.Interfaces.Enums;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL7Pdu : IComparable<IL7Pdu>
  {
    Int64 FirstSeenTicks { get; }
    Int64 LastSeenTicks { get; }

    DateTime FirstSeen { get; }
    DateTime LastSeen { get; }

    FlowDirection Direction { get; }
  
    Byte[] Payload { get; }  
    Int32 PayloadLen { get; }
  }
}
