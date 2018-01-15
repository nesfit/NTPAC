using System;
using Akka.Cluster.Sharding;

namespace NTPAC.Messages.Sharding
{
    public class RawPacketsMessageExtractor : IMessageExtractor
    {
        public String EntityId(Object message) => (message as RawPacketsShardEnvelope)?.EntityId.ToString();

        public Object EntityMessage(Object message) => (message as RawPacketsShardEnvelope)?.Message;

        public String ShardId(Object message) => (message as RawPacketsShardEnvelope)?.EntityId.ToString();
    }
}
