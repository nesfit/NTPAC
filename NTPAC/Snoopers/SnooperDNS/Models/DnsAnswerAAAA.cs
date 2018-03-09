using System.Net;

namespace SnooperDNS.Models
{
  public class DnsAnswerAAAA : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.Aaaa;
    
    public readonly IPAddress Address;

    public DnsAnswerAAAA(DnsPacket.AaaaRecord r) => this.Address = new IPAddress(r.Address);
  }
}
