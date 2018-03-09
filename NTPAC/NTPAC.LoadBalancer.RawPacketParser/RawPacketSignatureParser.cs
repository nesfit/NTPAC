using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.RawPacket;
using PacketDotNet;

namespace NTPAC.LoadBalancer.RawPacketParser
{
  // TODO: convert all outs to refs ? 
  
  [SuppressMessage("ReSharper", "RedundantAssignment")]
  public static class RawPacketSignatureParser
  {
    public static Boolean ExtractRawPacketSignature(RawPacket rawPacket,
                                                    out IPAddress sourceIpAddress, 
                                                    out IPAddress destinationIpAddress, 
                                                    out UInt16 sourcePort,
                                                    out UInt16 destinationPort,
                                                    out IPProtocolType ipProtocol,
                                                    out IPFragmentSignature ipv4FragmentSignature)
    {
      sourceIpAddress = destinationIpAddress = IPAddress.None;
      sourcePort = destinationPort = 0;
      ipProtocol = IPProtocolType.NONE;
      ipv4FragmentSignature = null;
      
      if (!ParseLinkLayer(rawPacket.RawPacketData, rawPacket.LinkType, out var networkData, out var ethernetProtocol))
      {
        return false;
      }
      
      if (!ParseNetworkLayer(networkData, ethernetProtocol, out var transportData,
                             out ipProtocol, out sourceIpAddress, out destinationIpAddress, out ipv4FragmentSignature))
      {
        return false;
      }

      // Don't continue with parsing of a transport layer, if the current packet is fragmented and this fragment is not the initial one (containing transport header) 
      if (ipv4FragmentSignature != null && ipv4FragmentSignature.FragmentOffset > 0)
      {
        return true;
      }
      
      if (!ParseTransportLayer(transportData, ipProtocol, out sourcePort, out destinationPort))
      {
        return false;
      }
      
      return true;
    }

    private static Boolean ParseLinkLayer(ReadOnlySpan<Byte> linkData,
                                          LinkLayers linkType, 
                                          out ReadOnlySpan<Byte> networkData,
                                          out EthernetPacketType ethernetProtocol)
    {
      networkData      = ArraySegment<Byte>.Empty;
      ethernetProtocol = EthernetPacketType.None;
      
      switch (linkType)
      {
        case LinkLayers.Ethernet:
          var ethernetTypePosition = EthernetFields.TypePosition;
          ethernetProtocol = (EthernetPacketType) ((linkData[ethernetTypePosition] << 8) | linkData[ethernetTypePosition + 1]);
          networkData = linkData.Slice(EthernetFields.HeaderLength, linkData.Length - EthernetFields.HeaderLength);    // 4B Ethernet trailer (CRC)
          return true;
        default:
          return false;
      }
    }

    private static Boolean ParseNetworkLayer(ReadOnlySpan<Byte> networkData, 
                                             EthernetPacketType ethernetProtocol,
                                             out ReadOnlySpan<Byte> transportData,
                                             out IPProtocolType ipProtocol, 
                                             out IPAddress sourceIpAddress, 
                                             out IPAddress destinationIpAddress,
                                             out IPFragmentSignature ipv4FragmentSignature)
    {
      transportData        = ReadOnlySpan<Byte>.Empty;
      ipProtocol           = IPProtocolType.NONE;
      sourceIpAddress      = IPAddress.None;
      destinationIpAddress = IPAddress.None;
      ipv4FragmentSignature = null;
      
      switch (ethernetProtocol)
      {
        case EthernetPacketType.IPv4:
          return ParseNetworkIpv4Layer(networkData, ref transportData, ref ipProtocol, ref sourceIpAddress, ref destinationIpAddress, ref ipv4FragmentSignature);
        case EthernetPacketType.IPv6:
          return ParseNetworkIpv6Layer(networkData, ref transportData, ref ipProtocol, ref sourceIpAddress, ref destinationIpAddress);
       default:
         return false;
      }
    }

    private static Boolean ParseNetworkIpv4Layer(ReadOnlySpan<Byte> networkData,
                                                 ref ReadOnlySpan<Byte> transportData,
                                                 ref IPProtocolType ipProtocol,
                                                 ref IPAddress sourceIpAddress,
                                                 ref IPAddress destinationIpAddress,
                                                 ref IPFragmentSignature ipv4FragmentSignature)
    {
      // Min ipv4 header size
      if (networkData.Length < 20)
      {
        return false;
      }
      
      ipProtocol = (IPProtocolType) networkData[IPv4Fields.ProtocolPosition];
          
      sourceIpAddress      = new IPAddress(networkData.Slice(IPv4Fields.SourcePosition, 4));
      destinationIpAddress = new IPAddress(networkData.Slice(IPv4Fields.DestinationPosition, 4));
          
      var headerLen = (networkData[0] & 0x0F) * 4;
      var totalLen =
        BinaryPrimitives.ReadUInt16BigEndian(networkData.Slice(IPv4Fields.TotalLengthPosition, IPv4Fields.TotalLengthLength));
      transportData = networkData.Slice(headerLen, totalLen - headerLen);

      
      var moreFragments = (networkData[IPv4Fields.FragmentOffsetAndFlagsPosition] & 0b00100000) != 0;
      var fragmentOffset = ((networkData[IPv4Fields.FragmentOffsetAndFlagsPosition] & 0b00011111) << 8) |
                           networkData[IPv4Fields.FragmentOffsetAndFlagsPosition + 1];
      var identification = BinaryPrimitives.ReadUInt16BigEndian(networkData.Slice(IPv4Fields.IdPosition, 2));
      var packetIsFragmented = moreFragments || fragmentOffset > 0;
      if (packetIsFragmented)
      {
        ipv4FragmentSignature = new IPFragmentSignature(sourceIpAddress, destinationIpAddress, ipProtocol, identification, moreFragments, fragmentOffset);
      }
      
      return true;
    }
    
    private static Boolean ParseNetworkIpv6Layer(ReadOnlySpan<Byte> networkData,
                                                 ref ReadOnlySpan<Byte> transportData,
                                                 ref IPProtocolType ipProtocol,
                                                 ref IPAddress sourceIpAddress,
                                                 ref IPAddress destinationIpAddress)
    {
      ipProtocol = (IPProtocolType) networkData[IPv6Fields.NextHeaderPosition];
          
      sourceIpAddress      = new IPAddress(networkData.Slice(IPv6Fields.SourceAddressPosition, 16));
      destinationIpAddress = new IPAddress(networkData.Slice(IPv6Fields.DestinationAddressPosition, 16));

      transportData = networkData.Slice(IPv6Fields.HeaderLength);
          
      return true;
    }

    private static Boolean ParseTransportLayer(ReadOnlySpan<Byte> transportData,
                                               IPProtocolType ipProtocol, 
                                               out UInt16 sourcePort, 
                                               out UInt16 destinationPort)
    {
      sourcePort      = 0;
      destinationPort = 0;
      
      if (transportData.Length < 8)
      {
        return false;
      }
      
      switch (ipProtocol)
      {
        case IPProtocolType.UDP:
          sourcePort      = BinaryPrimitives.ReadUInt16BigEndian(transportData.Slice(UdpFields.SourcePortPosition, 2));
          destinationPort = BinaryPrimitives.ReadUInt16BigEndian(transportData.Slice(UdpFields.DestinationPortPosition, 2));
          return true;
        case IPProtocolType.TCP:
          sourcePort      = BinaryPrimitives.ReadUInt16BigEndian(transportData.Slice(TcpFields.SourcePortPosition, 2));
          destinationPort = BinaryPrimitives.ReadUInt16BigEndian(transportData.Slice(TcpFields.DestinationPortPosition, 2));
          return true;
        default:
          return false;
      }
    }
  }
}
