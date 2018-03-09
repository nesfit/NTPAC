using System;
using SnooperDNS.Kaitai;

namespace SnooperDNS.Models
{
  public class DnsAnswerPTR : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.Ptr;
    
    public readonly String Hostname;
    
    public DnsAnswerPTR(DnsPacket.PtrRecord r) => this.Hostname = r.Hostname.ToJoinedString();
  }
}
