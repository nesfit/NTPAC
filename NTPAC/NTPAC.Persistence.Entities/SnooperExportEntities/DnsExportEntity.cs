using System;
using System.Collections.Generic;
using System.Net;

namespace NTPAC.Persistence.Entities.SnooperExportEntities
{
  public class DnsExportEntity : SnooperExportEntityBase
  {
    public Int32 TransactionId { get; set; }
    public SByte Type { get; set; }
    public IReadOnlyCollection<DnsQueryEntity> Queries { get; set; }
    public IReadOnlyCollection<DnsAnswerEntity> Answers { get; set; }

    public class DnsQueryEntity
    {
      public SByte Type { get; set; }
      public String Name { get; set; }
    }

    public class DnsAnswerEntity
    {
      public SByte Type { get; set; }
      public IPAddress Address { get; set; }
      public String Hostname { get; set; }
    }
  }
}
