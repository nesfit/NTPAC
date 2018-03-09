using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.ConversationTracking.Models;
using PacketDotNet;
using PacketDotNet.Utils;

namespace NTPAC.ConversationTracking.Factories
{
  public static class FrameFactory
  {
    public static Frame CreateFromPacket(Packet packet, Int64 timestamp)
    {
      var frame = CreateFrameBase(timestamp, packet);
      frame.IsValid = DissectSourcePacket(frame, packet);
      return frame;
    }

    public static Frame CreateFromIpPacket(IPv4Packet ipPacket, Int64 timestamp)
    {
      var frame = CreateFrameBase(timestamp, ipPacket);
      frame.IsValid = DissectSourceIpPacket(frame, ipPacket);
      return frame;
    }
    
    public static Frame CreateFromDefragmentedIpPacket(IPv4Packet ipPacket, IReadOnlyCollection<Frame> fragments)
    {
      var frame = CreateFrameBase(fragments.First().TimestampTicks, ipPacket.BytesHighPerformance.Bytes);
      frame.Ipv4Fragments = fragments;
      frame.IsValid = DissectSourceIpPacket(frame, ipPacket);
      return frame;
    }

    private static Frame CreateFrameBase(Int64 timestamp, Packet packet) =>
      CreateFrameBase(timestamp, packet.HeaderDataHighPerformance.Bytes);
    
    private static Frame CreateFrameBase(Int64 timestamp, Byte[] data)
      => new Frame(timestamp, data);
   
    private static Boolean DissectSourceIpPacket(Frame frame, IPPacket ipPacket)
    {
      frame.SourceAddress      = ipPacket.SourceAddress;
      frame.DestinationAddress = ipPacket.DestinationAddress;
      frame.IpProtocol         = ipPacket.Protocol;

      return TryDissectFragmentedIpPacket(frame, ipPacket) ||
             TryDissectNonTransportPacket(frame, ipPacket) ||
             TryDissectTransportPacket(frame, ipPacket);
    }

    private static Boolean DissectSourcePacket(Frame frame, Packet packet)
    {
      if (!(packet?.PayloadPacket is IPPacket ipPacket))
      {
        return false;
      }

      return DissectSourceIpPacket(frame, ipPacket);
    }

    private static void DissectTcpPacket(Frame frame, TcpPacket tcpPacket)
    {     
      frame.TcpFlags |= tcpPacket.Ack ? TcpFlags.Ack : 0; 
      frame.TcpFlags |= tcpPacket.CWR ? TcpFlags.Cwr : 0;
      frame.TcpFlags |= tcpPacket.ECN ? TcpFlags.Ecn : 0;
      frame.TcpFlags |= tcpPacket.Fin ? TcpFlags.Fin : 0;
      frame.TcpFlags |= tcpPacket.Psh ? TcpFlags.Psh : 0;
      frame.TcpFlags |= tcpPacket.Rst ? TcpFlags.Rst : 0;
      frame.TcpFlags |= tcpPacket.Syn ? TcpFlags.Syn : 0;
      frame.TcpFlags |= tcpPacket.Urg ? TcpFlags.Urg : 0; 

      frame.TcpSequenceNumber       = tcpPacket.SequenceNumber;
      frame.TcpAcknowledgmentNumber = tcpPacket.AcknowledgmentNumber;
      frame.TcpChecksum             = tcpPacket.Checksum;
      frame.TcpChecksumValid        = tcpPacket.ValidChecksum;
    }

    private static Boolean TryDissectFragmentedIpPacket(Frame frame, IPPacket ipPacket)
    {
      if (!(ipPacket is IPv4Packet ipv4Packet) || !Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
      {
        return false;
      }

      frame.IsIpv4Fragmented = true;
      frame.Ipv4FragmentKey      = new IPFragmentKey(ipv4Packet);
      frame.Ipv4FragmentOffset   = ipv4Packet.FragmentOffset * 8;
      frame.MoreIpv4Fragments    = (ipv4Packet.FragmentFlags & 0b001) != 0;

//      frame.SetupL7PayloadDataSegment(ipPacket.PayloadDataHighPerformance.Offset);
      SetupFrameL7PayloadDataSegmentFromPacketPayload(frame, ipPacket.HeaderDataHighPerformance);  
      
      return true;
    }

    private static Boolean TryDissectNonTransportPacket(Frame frame, IPPacket ipPacket)
    {
      if (ipPacket.PayloadPacket is TransportPacket)
      {
        return false;
      }

//      frame.L7Payload = ipPacket.PayloadData;
//      frame.SetupL7PayloadDataSegment(ipPacket.PayloadDataHighPerformance.Offset);
      SetupFrameL7PayloadDataSegmentFromPacketPayload(frame, ipPacket.BytesHighPerformance);
        
      return false;
    }

    private static Boolean TryDissectTransportPacket(Frame frame, IPPacket ipPacket)
    {
      if (!(ipPacket.PayloadPacket is TransportPacket transportPacket))
      {
        return false;
      }

      frame.SourcePort             = transportPacket.SourcePort;
      frame.DestinationPort        = transportPacket.DestinationPort;
      frame.IsValidTransportPacket = true;

      if (transportPacket is TcpPacket tcpPacket)
      {
        DissectTcpPacket(frame, tcpPacket);
      }
      
//      frame.L7Payload = ipPacket.PayloadPacket?.PayloadData;
//      frame.SetupL7PayloadDataSegment(transportPacket.BytesHighPerformance.Offset);
      SetupFrameL7PayloadDataSegmentFromPacketPayload(frame, transportPacket.PayloadDataHighPerformance
                                                             ?? transportPacket.HeaderDataHighPerformance);
      
      return true;
    }

    private static void SetupFrameL7PayloadDataSegmentFromPacketPayload(Frame frame, ByteArraySegment packetPayloadSegment)
    {
      if (packetPayloadSegment == null)
      {
        throw new ArgumentNullException(nameof(packetPayloadSegment));
      }
      frame.SetupL7PayloadDataSegment(packetPayloadSegment.Offset, packetPayloadSegment.Length);
    }
  }
}
