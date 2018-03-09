using System;
using SnooperDNS.Kaitai;

namespace SnooperDNS.Models
{
  public class DnsQuery
  {
    public readonly DnsPacket.RecordType Type;
    public readonly String Name;
    
    public DnsQuery(DnsPacket.Query dnsQuery)
    {
      this.Type = dnsQuery.Type;
      this.Name = dnsQuery.Name.ToJoinedString();
    }
  }
}
