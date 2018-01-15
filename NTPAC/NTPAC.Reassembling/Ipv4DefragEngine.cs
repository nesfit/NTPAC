using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NTPAC.Common.Models;
using PacketDotNet.IP;
using PacketDotNet.Tcp;
using PacketDotNet.Udp;
using PacketDotNet.Utils;
using SharpPcap;

namespace NTPAC.Reassembling
{
    public class Ipv4DefragEngine
    {
        private readonly ConcurrentDictionary<IPFragmentKey, ICollection<IPFragment>> _buffer = new ConcurrentDictionary<IPFragmentKey, ICollection<IPFragment>>();
        
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public Tuple<IEnumerable<RawCapture>, IPv4Packet> TryFragmentDefrag(IPFragment fragment)
        {
            var fragments = this._buffer.GetOrAdd(fragment.Key, _ => new HashSet<IPFragment>());
            // Gods of Akka, please dont damm me for using this lock
            lock (fragments)
            {
                fragments.Add(fragment);
                
                // Continue only if we have the final fragment  
                if (fragments.All(f => f.MoreFragments)) { return null; }
            
                // Sort fragmens by their offsets
                var sortedFragments = fragments.OrderBy(f => f.Offset);
            
                // Check whether we have all fragments
                var expectedOffset = 0;
                foreach (var storedFragment in sortedFragments)
                {
                    if (storedFragment.Offset != expectedOffset) { return null; }
                    var payloadLen = storedFragment.TotalLen - storedFragment.HeaderLen;
                    expectedOffset += payloadLen;
                }

                // Merge fragments payloads
                var defragmentedPayloadLen = expectedOffset;
                var defragmentedPayloadBuffer = new Byte[defragmentedPayloadLen]; 
                foreach (var storedFragment in sortedFragments)
                {
                    storedFragment.PayloadData.CopyTo(defragmentedPayloadBuffer, storedFragment.Offset);
                }
                
                var defragmentedIpv4Packet = new IPv4Packet(fragment.Key.SourceAddress, fragment.Key.DestinationAddress);
                // TODO tunneling support
                // Parse payload
                var payloadBas = new ByteArraySegment(defragmentedPayloadBuffer);
                switch (fragment.Protocol)
                {
                    // TCP and UDP packet constructors will setup references from their parents (defragmentedIPv4Packet) to themselves
                    case IPProtocolType.TCP:
                        new TcpPacket(payloadBas, defragmentedIpv4Packet);
                        break;
                    case IPProtocolType.UDP:
                        new UdpPacket(payloadBas, defragmentedIpv4Packet);
                        break;
                    default: throw new NotImplementedException();
                }

                this._buffer.TryRemove(fragment.Key, out _);
                
                return new Tuple<IEnumerable<RawCapture>, IPv4Packet>(sortedFragments.Select(f => f.RawCapture), defragmentedIpv4Packet);
            }
        }

        public static Boolean Ipv4PacketIsFragmented(IPv4Packet ipv4Packet)
        {
            var fragmentOffset = ipv4Packet.FragmentOffset;
            var moreFragments = (ipv4Packet.FragmentFlags & 0b100) != 0;
            return fragmentOffset != 0 || moreFragments;
        }
    }
}
