using System;
using System.Linq;

namespace SnooperDNS.Kaitai
{
  public static class DnsPacketExtensions
  {
    public static String ToJoinedString(this DnsPacket.DomainName domainName) =>
      String.Join(".", domainName.Labels.Select(l => l.Name));
  }
}
