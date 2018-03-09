using System;
using NTPAC.ConversationTracking.Models;

namespace NTPAC.Persistence.Entities.Mappers
{
  public static class L7ConversationMapper
  {
    public static L7ConversationEntity Map(L7Conversation l7Conversation) =>
      new L7ConversationEntity
      {
        DestinationEndPoint = new IPEndPointEntity(l7Conversation.DestinationEndPoint),
        ProtocolType        = (Int32) l7Conversation.ProtocolType,
        SourceEndPoint      = new IPEndPointEntity(l7Conversation.SourceEndPoint),
        FirstSeen           = l7Conversation.FirstSeen,
        LastSeen            = l7Conversation.LastSeen,
        Id                  = l7Conversation.Id,
        CaptureId           = l7Conversation.CaptureId,
        PduCount            = l7Conversation.Pdus.Count
      };
  }
}
