using System;
using NTPAC.ApplicationProtocolExport.Core.Exceptions;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;

namespace SnooperHTTP.Models
{
  public class HttpParsingException : SnooperExceptionBase
  {
    public HttpParsingException(String msg) : base(msg) { }
  }
}
