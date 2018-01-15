using System;
using MessagePack;

namespace NTPAC.Messages.Sharding
{
    [MessagePackObject]
    public class RawPacketsShardEnvelope
    {
        
        
        public RawPacketsShardEnvelope(Int32 entityId, ProcessRawPacketBatchRequest message)
        {
            this.EntityId = entityId;
            this.Message  = message;
        }

        [Key(0)]
        public Int32 EntityId { get; }

        [Key(1)]
        public ProcessRawPacketBatchRequest Message { get; }
    }
}
