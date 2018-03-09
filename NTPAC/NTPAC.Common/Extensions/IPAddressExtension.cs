using System;
using System.Net;

namespace NTPAC.Common.Extensions
{ 
  public static class IPAddressExtension
  {
    public static Int32 CompareTo(this IPAddress ipAddress1, IPAddress ipAddress2) 
    {
      if (ipAddress1.AddressFamily != ipAddress2.AddressFamily)
      {
        throw new ArgumentException($"Comparing IP address type {ipAddress1.AddressFamily.ToString()} with {ipAddress2.AddressFamily.ToString()}");
      }

      var ipAddress1Bytes = ipAddress1.GetAddressBytes();
      var ipAddress2Bytes = ipAddress2.GetAddressBytes();
      for (var i = 0; i < ipAddress1Bytes.Length; i++)
      {
        if (ipAddress1Bytes[i] < ipAddress2Bytes[i])
        {
          return -1;
        }
        if (ipAddress1Bytes[i] > ipAddress2Bytes[i])
        {
          return 1;
        } 
      }
      return 0;
    }
  }
}
