using System;

namespace NTPAC.ApplicationProtocolExport.Core.Exceptions
{
  public class KaitaiObjectionConstructionException : Exception
  {
    public KaitaiObjectionConstructionException(String message, Exception innerException) : base(message, innerException) { }
  }
}
