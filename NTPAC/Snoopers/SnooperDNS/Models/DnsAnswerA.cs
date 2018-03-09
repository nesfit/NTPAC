using System.Net;

namespace SnooperDNS.Models
{
  public class DnsAnswerA : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => DnsPacket.RecordType.A;
    
    public readonly IPAddress Address;

    public DnsAnswerA(DnsPacket.ARecord r) => this.Address = new IPAddress(r.Address);
  }
}
