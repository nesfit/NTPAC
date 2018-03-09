using System;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.Persistence.Entities.Mappers
{
  public static class L7PduMapper
  {
    public static L7PduEntity Map(IL7Pdu l7Pdu) =>
      new L7PduEntity
      {
        FirstSeenTicks = l7Pdu.FirstSeenTicks,
        LastSeenTicks  = l7Pdu.LastSeenTicks,
        Direction      = (SByte) l7Pdu.Direction,
        Payload        = l7Pdu.Payload
      };
  }
}
