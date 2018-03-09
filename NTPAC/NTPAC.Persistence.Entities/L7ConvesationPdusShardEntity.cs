using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Entities
{
  public class L7ConversationPdusShardEntity : IDataEntity
  {
    public Guid L7ConversationId { get; set; }
    public Int32 Shard { get; set; }
    public L7PduEntity[] Pdus { get; set; }
    
    // TODO get rid of
    public Guid Id { get; }
  }
}
