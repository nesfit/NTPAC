using System;
using System.Collections.Generic;

namespace NTPAC.Persistence.Entities.SnooperExportEntities
{
  public class HttpExportEntity : SnooperExportEntityBase
  {
    public SByte Type { get; set; }
    public String Method { get; set; }
    public String Uri { get; set; }
    public String Version { get; set; }
    public Int16 StatusCode { get; set; }
    public String StatusMessage { get; set; }
    public Dictionary<String, List<String>> HeaderFields { get; set; }
    public Byte[] Payload { get; set; }
    public Boolean PayloadIncomplete { get; set; }
  }
}
