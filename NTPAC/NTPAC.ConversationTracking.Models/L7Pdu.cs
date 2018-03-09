using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NTPAC.Common.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;

namespace NTPAC.ConversationTracking.Models
{
  public class L7Pdu : IL7Pdu
  {
    public FlowDirection Direction { get; }
    
    public Int64 FirstSeenTicks => this._frames.First().TimestampTicks;
    public Int64 LastSeenTicks => this._frames.Last().TimestampTicks;
    
    public DateTime FirstSeen => new DateTime(this.FirstSeenTicks);
    public DateTime LastSeen => new DateTime(this.LastSeenTicks);
    
    private readonly List<Frame> _frames;
    private Byte[] _payloadBuffer;
    
    public L7Pdu(Frame frame, FlowDirection direction)
    {
      this.Direction  = direction;
      this._frames    = new List<Frame>(1) {frame};
      this.PayloadLen = frame.L7PayloadDataSegmentLength;
    }
    
    public IReadOnlyList<Frame> Frames => this._frames;
    
    public Byte[] Payload
    {
      get
      {
        if (this._payloadBuffer == null)
        {
          this.RebuildPayloadDataBuffer();
        }

        return this._payloadBuffer;
      }
    }

    public Int32 PayloadLen { get; set; }

    public void AddFrame(Frame frame)
    {
      this._frames.Add(frame);
      this.PayloadLen += frame.L7PayloadDataSegmentLength;
      // Invalidate payload data buffer
      this._payloadBuffer = null;
    }

    private void RebuildPayloadDataBuffer()
    {
      var ms = new MemoryStream(new Byte[this.PayloadLen], 0, this.PayloadLen, true, true);
      foreach (var frame in this._frames)
      {
        var frameL7PayloadDataSegment = frame.L7PayloadDataSegment;
        var frameData = frameL7PayloadDataSegment.Array;
        ms.Write(frameData, frameL7PayloadDataSegment.Offset, frameL7PayloadDataSegment.Count);
      }

      this._payloadBuffer = ms.GetBuffer();
    }

    public Int32 CompareTo(IL7Pdu other) => this.FirstSeenTicks.CompareTo(other.FirstSeenTicks);

    public override String ToString() => $"{TimestampFormatter.Format(this.FirstSeenTicks)} > {this.Direction.ToString()} > {this.PayloadLen} B";    
  }
}
