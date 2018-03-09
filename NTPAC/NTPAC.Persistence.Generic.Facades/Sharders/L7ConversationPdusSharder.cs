using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.Mappers;

namespace NTPAC.Persistence.Generic.Facades.Sharders
{
  public static class L7ConversationPdusSharder
  {
    private const UInt32 MaximumL7ConversationPdusShardSize = 25_000_000; // 25MB, 134MB (128 MiB) Cassandra mutation limit

    public static IEnumerable<L7ConversationPdusShardEntity> ShardL7ConversationPdus(L7Conversation l7Conversation)
    {
      var currentShardNum = 0;      
      var currentShardSize = 0;
      var currentShardPdusBuffer = new List<L7PduEntity>();
      foreach (var pdu in l7Conversation.Pdus)
      {
        var pduEntity = L7PduMapper.Map(pdu);
        currentShardPdusBuffer.Add(pduEntity);
        currentShardSize += pduEntity.Payload.Length;

        if (currentShardSize < MaximumL7ConversationPdusShardSize)
        {
          continue;
        }

        yield return new L7ConversationPdusShardEntity
                     {
                       L7ConversationId = l7Conversation.Id,
                       Pdus             = currentShardPdusBuffer.ToArray(),
                       Shard            = currentShardNum
                     };

        currentShardSize = 0;
        currentShardNum++;
        currentShardPdusBuffer.Clear();
      }

      if (currentShardPdusBuffer.Any())
      {
        yield return new L7ConversationPdusShardEntity
                     {
                       L7ConversationId = l7Conversation.Id,
                       Pdus             = currentShardPdusBuffer.ToArray(),
                       Shard            = currentShardNum
                     };

      }
    }
  }
}
