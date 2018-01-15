using System;
using System.Collections.Generic;
using MessagePack;

namespace NTPAC.Messages
{
    [MessagePackObject]
    public class ProcessRawPacketBatchRequest
    {
        public ProcessRawPacketBatchRequest(List<ProcessRawPacketRequest> processRawPacketRequests, Int64 seqId)
        {
            this.ProcessRawPacketRequests = processRawPacketRequests;
            this.SeqId                    = seqId;
        }

        [Key(0)]
        public List<ProcessRawPacketRequest> ProcessRawPacketRequests { get; }

        [Key(1)]
        public Int64 SeqId { get; }
    }
}
