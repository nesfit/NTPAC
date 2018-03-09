using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using NTPAC.ConversationTracking.Interfaces;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Models
{
  public readonly struct L3L4ConversationKey
  {
    private readonly L3ConversationKeyStruct _l3Key;
    private readonly L4ConversationKeyStruct _l4Key;
    public IL3ConversationKey L3ConversationKey => new L3ConversationKeyClass(this._l3Key.IpAddress1, this._l3Key.IpAddress2);
    public IL4ConversationKey L4ConversationKey => new L4ConversationKeyClass(this._l4Key.Key, this._l4Key.Protocol);

    public L3L4ConversationKey(IPPacket sourceIpPacket)
    {
      if (sourceIpPacket == null)
      {
        throw new ArgumentNullException();
      }

      this._l3Key = new L3ConversationKeyStruct(sourceIpPacket);

      var payloadPacket = sourceIpPacket.PayloadPacket;
      switch (payloadPacket)
      {
        case TcpPacket tcpPacket:
          this._l4Key = new L4ConversationKeyStruct(tcpPacket.SourcePort, tcpPacket.DestinationPort, IPProtocolType.TCP);
          break;
        case UdpPacket udpPacket:
          this._l4Key = new L4ConversationKeyStruct(udpPacket.SourcePort, udpPacket.DestinationPort, IPProtocolType.UDP);
          break;
        default:
          throw new ArgumentException($"Unknown transport protocol payload: {payloadPacket}");
      }
    }

    public L3L4ConversationKey(Frame frame)
    {
      if (frame == null)
      {
        throw new ArgumentNullException();
      }
      this._l3Key = new L3ConversationKeyStruct(frame);
      this._l4Key = new L4ConversationKeyStruct(frame);
    }

    public L3L4ConversationKey(IPAddress sourceIpAddress,
                               IPAddress destinationIpAddress,
                               UInt16 sourcePort,
                               UInt16 destinationPort,
                               IPProtocolType ipProtocolType)
    {
      this._l3Key = new L3ConversationKeyStruct(sourceIpAddress, destinationIpAddress);
      this._l4Key = new L4ConversationKeyStruct(sourcePort, destinationPort, ipProtocolType);
    }

    public Boolean Equals(L3L4ConversationKey other) => this._l3Key == other._l3Key && this._l4Key == other._l4Key;
    

    public override Boolean Equals(Object obj)
    {
      if (obj is null)
      {
        return false;
      }

      return obj is L3L4ConversationKey key && this.Equals(key);
    }

    [SuppressMessage("ReSharper", "ImpureMethodCallOnReadonlyValueField")]
    public override Int32 GetHashCode()
    {
      unchecked
      {
        // Ensure only positive values by clearing the highest (sign) bit 
        return ((this._l3Key.GetHashCode() * 397) ^ this._l4Key.GetHashCode()) & 0x7fffffff;
      }
    }
  }
}
