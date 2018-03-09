using System;
using System.Net;
using NTPAC.Common.Enums;
using PacketDotNet.IP;

namespace NTPAC.Common.Models
{
    public interface IFrame
    {
        Int64 TimestampTicks { get; }
        IPAddress SourceAddress { get; }
        IPAddress DestinationAddress { get; }
        IPProtocolType IpProtocol { get; }
        UInt16 SourcePort { get; }
        UInt16 DestinationPort { get; }
        IPEndPoint SourceEndPoint { get; }
        IPEndPoint DestinationEndPoint { get; }
        L3ConversationKeyClass L3ConversationKey { get; }
        L4ConversationKeyClass L4ConversationKey { get; }
        Byte[] L7Payload { get; }
        Int32 L7PayloadLength { get; }
        TcpFlags TcpFlagsEnum { get; }
        Boolean TcpAck { get; }
        Boolean TcpCwr { get; }
        Boolean TcpEcn { get; }
        Boolean TcpFin { get; }
        Boolean TcpPsh { get; }
        Boolean TcpRst { get; }
        Boolean TcpSyn { get; }
        Boolean TcpUrg { get; }
        UInt32 TcpSequenceNumber { get; }
        UInt32 TcpAcknowledgmentNumber { get; }
        Boolean Valid { get; }
    }
}