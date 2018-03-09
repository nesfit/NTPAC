using System;
using NTPAC.Common.Extensions;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.Exceptions;

namespace NTPAC.Reassembling.TCP.Models
{
  public struct EffectiveFrame
  {
    public UInt64 RelativeOffsetBegin { get; private set; }
    public UInt64 RelativeOffsetEnd { get; private set; }
    public readonly Frame Frame;

    public EffectiveFrame(Frame frame, UInt32 seqNumOverflows = 0) : this()
    {
      this.Frame               = frame;
      this.RelativeOffsetBegin = frame.TcpSequenceNumber  + UInt32.MaxValue * (UInt64) seqNumOverflows;
      this.RelativeOffsetEnd   = this.RelativeOffsetBegin + (UInt64) frame.L7PayloadDataSegmentLength;
      
      this.NormalizeSequenceNumbersForKeepalive();
    }

    public Boolean Equals(EffectiveFrame other) => Equals(this.Frame, other.Frame);

    public override Boolean Equals(Object obj) => obj is EffectiveFrame frame && this.Equals(frame);

    public override Int32 GetHashCode() => this.Frame != null ? this.Frame.GetHashCode() : 0;

    public UInt64 PayloadLen => this.RelativeOffsetEnd - this.RelativeOffsetBegin;

    public Boolean PayloadsOverlapMismatch(EffectiveFrame other)
    {
      // Check if effective frames are overlapping
      if (this.RelativeOffsetBegin > other.RelativeOffsetEnd || other.RelativeOffsetBegin > this.RelativeOffsetEnd)
      {
        // If there is no overlap, there is no mismatch
        return false;
      }

      if (this.Frame.IsTcpKeepAlive || other.Frame.IsTcpKeepAlive)
      {
        return false;
      }
      
      
      var overlapBegin = Math.Max(this.RelativeOffsetBegin, other.RelativeOffsetBegin);
      var overlapEnd   = Math.Min(this.RelativeOffsetEnd, other.RelativeOffsetEnd);
      var overlapLen   = (Int32) (overlapEnd - overlapBegin);

      var payloadOffset      = (Int32) (overlapBegin - this.RelativeOffsetBegin);
      var otherPayloadOffset = (Int32) (overlapBegin - other.RelativeOffsetBegin);

      var payloadDataSegment      = this.Frame.L7PayloadDataSegment;
      var otherPayloadDataSegment = other.Frame.L7PayloadDataSegment;

      return !payloadDataSegment.ContentsEqual(otherPayloadDataSegment, payloadOffset, otherPayloadOffset, overlapLen);
    }

    private void NormalizeSequenceNumbersForKeepalive()
    {
      if (!this.Frame.IsTcpKeepAlive)
      {
        return;
      }
      
      var l7PayloadLen = this.Frame.L7PayloadDataSegmentLength;
      if (!(l7PayloadLen == 0 || l7PayloadLen == 1))
      {
        throw new ReassemblingException(this.Frame, $"Invalid keepalive length. Got {l7PayloadLen}, expected 0 or 1");
      }

      // Normalize sequence numbers for keepalives with single data byte (these have sequence numbers decreased by 1)
      if (l7PayloadLen == 1)
      {
        
        this.RelativeOffsetBegin++;
        this.RelativeOffsetEnd++;  
      }
      // Keep keepalives with no data bytes as the are 
    }
    
    public override String ToString() => $"Seq:{this.RelativeOffsetBegin}-{this.RelativeOffsetEnd} > {this.Frame}";
  }
}
