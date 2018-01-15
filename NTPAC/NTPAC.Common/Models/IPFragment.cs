using System;
using System.Runtime.CompilerServices;
using PacketDotNet.IP;
using SharpPcap;

namespace NTPAC.Common.Models
{
    public class IPFragment
    {
        public IPFragment(IPv4Packet ipv4Packet, RawCapture rawCapture)
        {
            this.RawCapture = rawCapture;
            this.Key = new IPFragmentKey(ipv4Packet);
            
            this.Offset        = ipv4Packet.FragmentOffset * 8;
            this.MoreFragments = (ipv4Packet.FragmentFlags & 0b100) != 0;
            this.PayloadData = ipv4Packet.PayloadData;
            this.HeaderLen = ipv4Packet.HeaderLength;
        }

        public readonly RawCapture RawCapture;
        public readonly IPFragmentKey Key;
        
        public readonly Int32 Offset;
        public readonly Boolean MoreFragments;
        public readonly Int32 HeaderLen;
        public readonly Byte[] PayloadData;
        public Int32 PayloadDataLen => this.PayloadData.Length;
        public Int32 TotalLen => this.HeaderLen + this.PayloadDataLen;
        public IPProtocolType Protocol => this.Key.Protocol;
    }
}
