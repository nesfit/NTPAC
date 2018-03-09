namespace SnooperDNS.Models
{
  public class DnsAnswerOther : DnsAnswerBase
  {
    public override DnsPacket.RecordType Type => 0;
  }
}
