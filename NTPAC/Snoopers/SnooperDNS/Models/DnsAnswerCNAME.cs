using System;
using SnooperDNS.Kaitai;

namespace SnooperDNS.Models
{
  public class DnsAnswerCNAME : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.Cname;
    
    public readonly String Hostname;

    public DnsAnswerCNAME(DnsPacket.CnameRecord r) => this.Hostname = r.Hostname.ToJoinedString();
  }
}
