using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using NTPAC.Common.Enums;
using PacketDotNet;
using PacketDotNet.Interfaces;
using PacketDotNet.IP;
using PacketDotNet.Tcp;

namespace NTPAC.Common.Models
{
    public class Frame
    {        
        // Restore from ordinary packet
        public Frame(Packet packet, Int64 timestamp)
        {
            this.TimestampTicks = timestamp;
            this.Valid = this.DissectSourcePacket(packet);
        }

        // Restore from defragmented IPv4 packet
        public Frame(IPv4Packet defragmentedIpv4Packet, Int64 timestamp)
        {
            this.TimestampTicks = timestamp;
            this.Valid = this.DissectSourceIpPacket(defragmentedIpv4Packet);
        }
        
        private Boolean DissectSourcePacket(Packet packet)
        {
            if (!(packet?.PayloadPacket is IpPacket ipPacket)) { return false; }
            return this.DissectSourceIpPacket(ipPacket);
        }
        
        private Boolean DissectSourceIpPacket(IpPacket ipPacket)
        {
            this.SourceAddress = ipPacket.SourceAddress;
            this.DestinationAddress = ipPacket.DestinationAddress;
            this.IpProtocol = ipPacket.Protocol;

            if (!(ipPacket.PayloadPacket is ISourceDestinationPort transportPacket)) { return false; }
            this.SourcePort = transportPacket.SourcePort;
            this.DestinationPort = transportPacket.DestinationPort;
            if (transportPacket is TcpPacket tcpPacket) { this.DissectSourceTcpPacket(tcpPacket); }

            this.L7Payload = (transportPacket as TransportPacket)?.PayloadData;

            this.L3ConversationKey = new L3ConversationKeyClass(this.SourceAddress, this.DestinationAddress);
            this.L4ConversationKey = new L4ConversationKeyClass(this.SourcePort, this.DestinationPort, this.IpProtocol);
    
            return true;
        }

        private void DissectSourceTcpPacket(TcpPacket tcpPacket)
        {
            this.TcpFlagsEnum |= tcpPacket.Fin ? TcpFlags.Fin : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.Syn ? TcpFlags.Syn : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.Rst ? TcpFlags.Rst : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.Psh ? TcpFlags.Psh : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.Ack ? TcpFlags.Ack : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.Urg ? TcpFlags.Urg : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.ECN ? TcpFlags.Ecn : TcpFlags.None;
            this.TcpFlagsEnum |= tcpPacket.CWR ? TcpFlags.Cwr : TcpFlags.None;

            this.TcpSequenceNumber = tcpPacket.SequenceNumber;
            this.TcpAcknowledgmentNumber = tcpPacket.AcknowledgmentNumber;
        }
        

        public Int64 TimestampTicks { get; }

        public Boolean Valid { get; }

        public IPAddress SourceAddress { get; private set; }
        public IPAddress DestinationAddress { get; private set; }
        public IPProtocolType IpProtocol { get; private set; }

        public UInt16 SourcePort { get; private set; }
        public UInt16 DestinationPort { get; private set; }

        public Byte[] L7Payload { get; protected set; }
        public Int32 L7PayloadLength => this.L7Payload?.Length ?? 0;

        public L3ConversationKeyClass L3ConversationKey { get; private set; }
        public L4ConversationKeyClass L4ConversationKey { get; private set; }

        private IPEndPoint _sourceEndPoint;
        public IPEndPoint SourceEndPoint
        {
            get
            {
                if (this._sourceEndPoint == null) {
                    this._sourceEndPoint = new IPEndPoint(this.SourceAddress, this.SourcePort); }
                return this._sourceEndPoint;
            }
        }
        
        private IPEndPoint _destinationEndPoint;
        public IPEndPoint DestinationEndPoint
        {
            get
            {
                if (this._destinationEndPoint == null) {
                    this._destinationEndPoint = new IPEndPoint(this.DestinationAddress, this.DestinationPort); }
                return this._destinationEndPoint;
            }
        }
        
        public TcpFlags TcpFlagsEnum { get; private set; }
        public Boolean TcpFin => this.TcpFlagsEnum == TcpFlags.Fin;
        public Boolean TcpSyn => this.TcpFlagsEnum == TcpFlags.Syn;
        public Boolean TcpRst => this.TcpFlagsEnum == TcpFlags.Rst;
        public Boolean TcpPsh => this.TcpFlagsEnum == TcpFlags.Psh;
        public Boolean TcpAck => this.TcpFlagsEnum == TcpFlags.Ack;
        public Boolean TcpUrg => this.TcpFlagsEnum == TcpFlags.Urg;
        public Boolean TcpEcn => this.TcpFlagsEnum == TcpFlags.Ecn;
        public Boolean TcpCwr => this.TcpFlagsEnum == TcpFlags.Cwr;

        public UInt32 TcpSequenceNumber { get; private set; }
        public UInt32 TcpAcknowledgmentNumber { get; private set; }
    }
}
