using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Entities.SnooperExportEntities
{
  public abstract class SnooperExportEntityBase : IDataEntity
  {
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public String Snooper { get; set; }
    
    public Guid L7ConversationId { get; set; }
    public DateTime Timestamp { get; set; }
    public SByte Direction { get; set; }
  }
}
