using System;
using System.Diagnostics.CodeAnalysis;
using PacketDotNet;
using PacketDotNet.IP;
using PacketDotNet.Tcp;
using PacketDotNet.Udp;

namespace NTPAC.Common.Models
{
    public struct L3L4ConversationKey
    {
        private readonly L3ConversationKeyStruct _l3Key;
        private readonly L4ConversationKeyStruct _l4Key;

        public L3ConversationKeyClass L3ConversationKey => new L3ConversationKeyClass(this._l3Key.Address1, this._l3Key.Address2);

        public L4ConversationKeyClass L4ConversationKey => new L4ConversationKeyClass(this._l4Key.Key, this._l4Key.Protocol);

        public L3L4ConversationKey(IpPacket sourceIpPacket)
        {
            this._l3Key = new L3ConversationKeyStruct(sourceIpPacket);

            switch (sourceIpPacket?.PayloadPacket)
            {
                case TcpPacket tcpPacket:
                    this._l4Key = new L4ConversationKeyStruct(tcpPacket.SourcePort, tcpPacket.DestinationPort, IPProtocolType.TCP);
                    break;
                case UdpPacket udpPacket:
                    this._l4Key = new L4ConversationKeyStruct(udpPacket.SourcePort, udpPacket.DestinationPort, IPProtocolType.UDP);
                    break;
                default:
                    //this._l4Key = new L4ConversationKeyStruct(0, 0, IPProtocolType.TCP);
                    //break;
                    throw new NotImplementedException();
            }
        }

        public Boolean Equals(L3L4ConversationKey other) => this._l3Key == other._l3Key && this._l4Key == other._l4Key;

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
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
                return (this._l3Key.GetHashCode() * 397) ^ this._l4Key.GetHashCode();
            }
        }
    }
}
