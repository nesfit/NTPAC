using System;
using NTPAC.ApplicationProtocolExport.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Core.Models
{
  public class SnooperUnhandledExceptionExport : SnooperExportBase
  {
    public SnooperUnhandledExceptionExport(IPduReader pduReader, Exception exception) : base(pduReader)
      => this.ParsingError = exception;
  }
}
