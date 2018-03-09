using System;
using NTPAC.ApplicationProtocolExport.Core.Exceptions;

namespace SnooperTLS.Models
{
  public class TlsParsingException : SnooperExceptionBase
  {
    public TlsParsingException(String msg) : base(msg) { }
  }
}
