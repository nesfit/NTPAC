namespace SnooperDNS.Models
{
  public abstract class DnsAnswerBase
  {
    public abstract DnsPacket.RecordType Type { get; }
  }
}
