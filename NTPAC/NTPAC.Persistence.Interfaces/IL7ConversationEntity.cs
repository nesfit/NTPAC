using System;
using System.Net;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Interfaces
{
  public interface IL7ConversationEntity : IDataEntity
  {    
    Guid CaptureId { get; set; }
    DateTime FirstSeen { get; set; }
    DateTime LastSeen { get; set; }
    Int32 ProtocolType { get; set; }
    Int64 PduCount { get; set; }
    
    IPAddress SourceIPAddress { get; }
    Int32 SourcePort { get; }
    IPAddress DestinationIPAddress { get; }
    Int32 DestinationPort { get; }
  }
}
