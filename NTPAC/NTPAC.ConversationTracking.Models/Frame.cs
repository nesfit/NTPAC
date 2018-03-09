using System;
using System.Collections.Generic;
using System.Net;
using Akka.Util.Internal;
using NTPAC.Common.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Models
{
  public class Frame : IComparable<Frame>, IEquatable<Frame>
  {
    public readonly Int64 TimestampTicks; 

    private Byte[] _data;

    public Frame(Int64 timestampTicks, Byte[] data)
    {
      this.TimestampTicks = timestampTicks;
      this.Data = data;
    }
    
    public Byte[] Data
    {
      get => this._data;
      set {
        this._data = value;
        if (this._l7PayloadDataSegmentSet)
        {
          // Invalidate L7PayloadDataSegment (set new instance to lose reference to previous Data object)
          this._l7PayloadDataSegmentSet = false;
          this._l7PayloadDataSegment    = new ArraySegment<Byte>();  
        }
      }
    }

    public Boolean IsValid { get; set; }
    
    public IPAddress SourceAddress { get; set; }
    public IPAddress DestinationAddress { get; set; }
    
    public UInt16 SourcePort { get; set; }
    public UInt16 DestinationPort { get; set; }
    
    public IPProtocolType IpProtocol { get; set; }
    
    private IPEndPoint _sourceEndPoint;
    public IPEndPoint SourceEndPoint =>
      this._sourceEndPoint ?? (this._sourceEndPoint = new IPEndPoint(this.SourceAddress, this.SourcePort));
    
    private IPEndPoint _destinationEndPoint;
    public IPEndPoint DestinationEndPoint =>
      this._destinationEndPoint ?? (this._destinationEndPoint = new IPEndPoint(this.DestinationAddress, this.DestinationPort));

    private IL3ConversationKey _l3ConversationKey;
    public IL3ConversationKey L3ConversationKey =>
      this._l3ConversationKey ??
      (this._l3ConversationKey = new L3ConversationKeyClass(this.SourceAddress, this.DestinationAddress));

    private IL4ConversationKey _l4ConversationKey;
    public IL4ConversationKey L4ConversationKey =>
      this._l4ConversationKey ??
      (this._l4ConversationKey = new L4ConversationKeyClass(this.SourcePort, this.DestinationPort, this.IpProtocol));

    private Boolean _l7PayloadDataSegmentSet;
    private ArraySegment<Byte> _l7PayloadDataSegment;

    public ArraySegment<Byte> L7PayloadDataSegment
    {
      get
      {
        if (!this._l7PayloadDataSegmentSet)
        {
          throw new InvalidOperationException("L7PayloadDataSegment not set");
        }

        return this._l7PayloadDataSegment;
      }
      private set
      {
        this._l7PayloadDataSegment    = value;
        this._l7PayloadDataSegmentSet = true;
      }
    }

    public Int32 L7PayloadDataSegmentLength => this.L7PayloadDataSegment.Count;
    
    public Boolean IsIpv4Fragmented { get; set; }
    public Boolean MoreIpv4Fragments { get; set; }
    public Int32 Ipv4FragmentOffset { get; set; }
    public IPFragmentKey Ipv4FragmentKey { get; set; }
    
    public Boolean IsIpv4Defragmented => this.Ipv4Fragments != null;
    public IReadOnlyCollection<Frame> Ipv4Fragments { get; set; }
    
    public UInt32 TcpAcknowledgmentNumber { get; set; }
    public UInt16 TcpChecksum { get; set; }
    public Boolean TcpChecksumValid { get; set; }
    public UInt32 TcpSequenceNumber { get; set; }
    
    public Boolean IsValidTransportPacket { get; set; }
    
    public TcpFlags TcpFlags;
    public Boolean TcpFAck => (this.TcpFlags & TcpFlags.Ack) != 0;
    public Boolean TcpFCwr => (this.TcpFlags & TcpFlags.Cwr) != 0;
    public Boolean TcpFEcn => (this.TcpFlags & TcpFlags.Ecn) != 0;
    public Boolean TcpFFin => (this.TcpFlags & TcpFlags.Fin) != 0;
    public Boolean TcpFPsh => (this.TcpFlags & TcpFlags.Psh) != 0;
    public Boolean TcpFRst => (this.TcpFlags & TcpFlags.Rst) != 0;
    public Boolean TcpFSyn => (this.TcpFlags & TcpFlags.Syn) != 0;
    public Boolean TcpFUrg => (this.TcpFlags & TcpFlags.Urg) != 0;
    
    public Boolean IsTcpRetransmission { get; set; }
    public Boolean IsTcpKeepAlive { get; set; }

    public Int32 CompareTo(Frame other) => this.TimestampTicks.CompareTo(other.TimestampTicks);

    public override String ToString() =>
      $"{TimestampFormatter.Format(this.TimestampTicks)} > {this.SourceEndPoint}-{this.DestinationEndPoint} {this.TcpFlags.ToString()} > {(this._l7PayloadDataSegmentSet ? this._l7PayloadDataSegment.Count.ToString() : "-")}/{this.Data.Length} B / {this.AnalysisInfo()}";
    
    private String AnalysisInfo()
    {
      var options = new List<String>();
      
      if (this.IsTcpKeepAlive)
      {
        options.Add("KA");
      }

      if (this.IsTcpRetransmission)
      {
        options.Add("RT");
      }

      if (this.IsIpv4Fragmented)
      {
        options.Add("FR");
      }

      if (this.IsIpv4Defragmented)
      {
        options.Add("DFR");
      }
      
      return options.Join(",");
    }
    
    public void SetupL7PayloadDataSegment(Int32 payloadOffset, Int32 payloadLength)
    {
      if (this.Data == null)
      {
        throw new InvalidOperationException("Data not set");
      }
      
      this.L7PayloadDataSegment = new ArraySegment<Byte>(this.Data, payloadOffset, payloadLength);
    }
    
    public Boolean Equals(Frame other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return Equals(this.Data, other.Data) && this.TimestampTicks == other.TimestampTicks;
    }

    public override Int32 GetHashCode()
    {
      unchecked
      {
        return ((this.Data != null ? this.Data.GetHashCode() : 0) * 397) ^ this.TimestampTicks.GetHashCode();
      }
    }
  }
}
