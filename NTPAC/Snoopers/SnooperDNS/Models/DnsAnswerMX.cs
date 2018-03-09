using System;
using SnooperDNS.Kaitai;

namespace SnooperDNS.Models
{
  public class DnsAnswerMX : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.Mx;
    
    public readonly String Hostname;
    
    public DnsAnswerMX(DnsPacket.MxRecord r) => this.Hostname = r.Hostname.ToJoinedString();
  }
}
