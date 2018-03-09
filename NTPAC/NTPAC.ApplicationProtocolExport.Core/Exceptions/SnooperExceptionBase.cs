using System;

namespace NTPAC.ApplicationProtocolExport.Core.Exceptions
{
  public abstract class SnooperExceptionBase : Exception
  {
    protected SnooperExceptionBase(String msg) : base(msg) { }
  }
}
