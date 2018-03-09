using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.Common.Collections;
using NTPAC.Common.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.ConversationTracking.Models
{
  public class L7Conversation : IL7Conversation
  {
    private static readonly IReadOnlyCollection<L7Pdu> NoPdusPlaceholder = new L7Pdu[0];
    private static readonly IReadOnlyCollection<Frame> NoFramesPlaceholder = new Frame[0];
    
    public L7Conversation(IPEndPoint sourceEndPoint,
                          IPEndPoint destinationEndPoint,
                          IPProtocolType protocolType,
                          L7Flow upL7Flow,
                          L7Flow downL7Flow)
    {
      this.SourceEndPoint      = sourceEndPoint;
      this.DestinationEndPoint = destinationEndPoint;
      this.ProtocolType        = protocolType;      
      
      this._upPdus   = upL7Flow?.L7Pdus   ?? NoPdusPlaceholder;
      this._downPdus = downL7Flow?.L7Pdus ?? NoPdusPlaceholder;
      this._pdus = new LazilyMergedCollection<L7Pdu>(this._upPdus, this._downPdus);

      this.UpNonDataFrames = upL7Flow?.NonDataFrames ?? NoFramesPlaceholder;
      this.DownNonDataFrames = downL7Flow?.NonDataFrames ?? NoFramesPlaceholder;
      this.NonDataFrames = new LazilyMergedCollection<Frame>(this.UpNonDataFrames, this.DownNonDataFrames);

      this.FirstSeenTicks = Math.Min(upL7Flow?.FirstSeenTicks ?? Int64.MaxValue, downL7Flow?.FirstSeenTicks ?? Int64.MaxValue);
      // Handle case where both timestamps are missing
      if (this.FirstSeenTicks == Int64.MaxValue)
      {
        this.FirstSeenTicks = 0;
      }

      this.LastSeenTicks = Math.Max(upL7Flow?.LastSeenTicks ?? 0, downL7Flow?.LastSeenTicks ?? 0);
    }

    public Guid Id { get; } = Guid.NewGuid();
    
    public IPProtocolType ProtocolType { get; }
    public IPEndPoint SourceEndPoint { get; }
    public IPEndPoint DestinationEndPoint { get; }

    private readonly IReadOnlyCollection<L7Pdu> _upPdus;
    private readonly IReadOnlyCollection<L7Pdu> _downPdus;
    private readonly IReadOnlyCollection<L7Pdu> _pdus;

    public IEnumerable<IL7Pdu> UpPdus => this._upPdus;
    public IEnumerable<IL7Pdu> DownPdus => this._downPdus;
    public IReadOnlyCollection<IL7Pdu> Pdus => this._pdus;
    
    public IReadOnlyCollection<Frame> UpNonDataFrames { get; }
    public IReadOnlyCollection<Frame> DownNonDataFrames { get; }
    public IReadOnlyCollection<Frame> NonDataFrames { get; }

    public Int64 FirstSeenTicks { get; }
    public DateTime FirstSeen => this.FirstSeenTicks > 0 ? new DateTime(this.FirstSeenTicks).ToLocalTime() : DateTime.MinValue;
    
    public Int64 LastSeenTicks { get; }
    public DateTime LastSeen => this.LastSeenTicks > 0 ? new DateTime(this.LastSeenTicks).ToLocalTime() : DateTime.MinValue;
    
    public Guid CaptureId { get; set; }

    public override String ToString() =>
      $"{TimestampFormatter.Format(this.FirstSeen)} > {this.ProtocolType} {this.SourceEndPoint}-{this.DestinationEndPoint} > UpPDUs:{this._upPdus.Count} DownPDUs:{this._downPdus.Count}";

    
    #region RawConversationBuilder
    
    public IEnumerable<RawCapture> ReconstructRawCaptures()
    {
      var frames = GetRealFrames();
      return frames.Select(frame =>
      {
        var secs  = frame.TimestampTicks                                    / TimeSpan.TicksPerSecond;
        var usecs = (frame.TimestampTicks - secs * TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerMillisecond / 1000);
        return new RawCapture(LinkLayers.Ethernet, new PosixTimeval((UInt64) secs, (UInt64) usecs), frame.Data);
      });
    }
    
    private IEnumerable<Frame> GetRealFrames()
    {
      var upDataFrames = GetRealFrames(this._upPdus);
      var downDataFrames = GetRealFrames(this._downPdus);
      var dataFrames = new LazilyMergedEnumerable<Frame>(upDataFrames, downDataFrames);

      var upNonDataFrames = GetRealFrames(this.UpNonDataFrames);
      var downNonDataFrames = GetRealFrames(this.DownNonDataFrames);
      var nonDataFrames = new LazilyMergedEnumerable<Frame>(upNonDataFrames, downNonDataFrames);
      
      var frames = new LazilyMergedEnumerable<Frame>(dataFrames, nonDataFrames);

      return frames;
    }

    private static IEnumerable<Frame> GetRealFrames(IEnumerable<L7Pdu> flowPdus)
    {
      var q = new Queue<Frame>();
      
      foreach (var pdu in flowPdus)
      {
        foreach (var frame in GetRealFrames(pdu.Frames, q))
        {
          yield return frame;
        }
      }
    }
    
    private static IEnumerable<Frame> GetRealFrames(IEnumerable<Frame> frames)
    {
      var q = new Queue<Frame>();
      return GetRealFrames(frames, q);
    }

    private static IEnumerable<Frame> GetRealFrames(IEnumerable<Frame> frames, Queue<Frame> q)
    {
      q.Clear();

      foreach (var frame in frames)
      {
        q.Enqueue(frame);

        while (q.Any())
        {
          var f = q.Dequeue();

          if (f.IsIpv4Defragmented)
          {
            foreach (var fragment in f.Ipv4Fragments)
            {
              q.Enqueue(fragment);
            }
          }
          else
          {
            yield return f;
          }
        }
      }     
    }
    
    #endregion
  }
}
