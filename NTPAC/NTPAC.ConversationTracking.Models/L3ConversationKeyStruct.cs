using System;
using System.Net;
using NTPAC.Common.Extensions;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Models
{
  public readonly struct L3ConversationKeyStruct
  {
    public IPAddress IpAddress1 { get; }
    public IPAddress IpAddress2 { get; }

    public L3ConversationKeyStruct(IPAddress ipAddress1, IPAddress ipAddress2) => (this.IpAddress1, this.IpAddress2) = OrderIpAddresses(ipAddress1, ipAddress2);

    public L3ConversationKeyStruct(Frame frame) : this(frame.SourceAddress, frame.DestinationAddress)
    {
    } 

    public L3ConversationKeyStruct(IPPacket ipPacket) : this(ipPacket.SourceAddress, ipPacket.DestinationAddress) { }

    public static Boolean Equals(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) =>
      key1.IpAddress1.Equals(key2.IpAddress1) && key1.IpAddress2.Equals(key2.IpAddress2);

    public override Boolean Equals(Object obj)
    {
      if (obj is null)
      {
        return false;
      }

      return obj.GetType() == this.GetType() && this.Equals((L3ConversationKeyStruct) obj);
    }

    public override Int32 GetHashCode()
    {
      unchecked
      {
        return (this.IpAddress1?.GetHashCode() ?? 0 * 397) ^ (this.IpAddress2?.GetHashCode() ?? 0) ;
      }
    }

    public static Boolean operator ==(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) => Equals(key1, key2);
    public static Boolean operator !=(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) => !Equals(key1, key2);
    public override String ToString() => $"{this.IpAddress1}:{this.IpAddress2}";

    public Boolean Equals(L3ConversationKeyStruct other) =>
      Equals(this.IpAddress1, other.IpAddress1) && Equals(this.IpAddress2, other.IpAddress2);

    private static (IPAddress ipAddress1, IPAddress ipAddress2) OrderIpAddresses(IPAddress ipAddress1, IPAddress ipAddress2) =>
      ipAddress1.CompareTo(ipAddress2) <= 0 ? (ipAddress1, ipAddress2) : (ipAddress2, ipAddress1);
  }
}
