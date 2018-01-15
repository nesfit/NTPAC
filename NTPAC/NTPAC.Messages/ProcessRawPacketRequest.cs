using System;
using MessagePack;
using NTPAC.Common.Models;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.Messages
{
    [MessagePackObject]
    public class ProcessRawPacketRequest
    {
        
        public ProcessRawPacketRequest(RawCapture rawCapture)
        {
            this.DateTimeTicks = rawCapture.Timeval.Date.Ticks;
            this.LinkType = rawCapture.LinkLayerType;
            this.RawPacketData = rawCapture.Data;
        }

        public ProcessRawPacketRequest(RawCapture rawCapture, L3L4ConversationKey l3L4ConversationKey, Int32 maxNumberOfShards) :
            this(rawCapture) =>
            this.EntityId = Math.Abs(l3L4ConversationKey.GetHashCode() % maxNumberOfShards);

        public ProcessRawPacketRequest() { }

        [Key(0)]
        public Int64 DateTimeTicks { get; }

        [IgnoreMember]
        public Int32 EntityId { get; }

        [Key(1)]
        public LinkLayers LinkType { get; }

        [Key(2)]
        public Byte[] RawPacketData { get; }

        public RawCapture RecreateOriginalRawCapture()
        {
            var secs = this.DateTimeTicks / TimeSpan.TicksPerSecond;
            var usecs = (this.DateTimeTicks - secs * TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerMillisecond / 1000); 
            return new RawCapture(this.LinkType, new PosixTimeval((UInt64)secs, (UInt64)usecs), this.RawPacketData);  
        }  
        
        public override String ToString() => $"{nameof(this.DateTimeTicks)}: {new DateTime(this.DateTimeTicks)}, {nameof(this.EntityId)}: {this.EntityId}, {nameof(this.LinkType)}: {this.LinkType}";
    }
}
