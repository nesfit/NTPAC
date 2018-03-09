using System;
using Cassandra.Mapping;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.SnooperExportEntities;

namespace NTPAC.Persistence.Cassandra.Facades
{
  public class CassandraMappings : Mappings
  {
    public CassandraMappings()
    {
      this.For<CaptureEntity>().TableName(typeof(CaptureEntity).Name).PartitionKey(e => e.Id);

      this.For<L7ConversationEntity>().TableName(typeof(L7ConversationEntity).Name).PartitionKey(e => e.Id)
        .Column(e => e.ProtocolType, c => c.WithName(nameof(L7ConversationEntity.ProtocolType)).WithDbType<Int32>())
        .Column(e => e.CaptureId, c => c.WithSecondaryIndex());

      this.For<L7ConversationPdusShardEntity>().TableName(typeof(L7ConversationPdusShardEntity).Name)
        .PartitionKey(e => e.L7ConversationId).ClusteringKey(e => e.Shard)
        .Column(e => e.Pdus, c => c.WithName(nameof(L7ConversationPdusShardEntity.Pdus)).AsFrozen());

      this.For<HttpExportEntity>().TableName(typeof(HttpExportEntity).Name).PartitionKey(e => e.Id)
          .Column(e => e.HeaderFields, c => c.WithName(nameof(HttpExportEntity.HeaderFields)).AsFrozen());

      this.For<DnsExportEntity>().TableName(typeof(DnsExportEntity).Name).PartitionKey(e => e.Id)
          .Column(e => e.Queries, c => c.WithName(nameof(DnsExportEntity.Queries)).AsFrozen())
          .Column(e => e.Answers, c => c.WithName(nameof(DnsExportEntity.Answers)).AsFrozen());

      this.For<GenericExportEntity>().TableName(typeof(GenericExportEntity).Name).PartitionKey(e => e.Id);
    }
  }
}
