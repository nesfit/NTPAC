using System;
using SnooperDNS.Kaitai;

namespace SnooperDNS.Models
{
  public class DnsAnswerNS : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.Ns;
    
    public readonly String Hostname;
    
    public DnsAnswerNS(DnsPacket.NsRecord r) => this.Hostname = r.Hostname.ToJoinedString();
  }
}
