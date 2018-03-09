using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.Common.Interfaces;
using NTPAC.ConversationTracking.Factories;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling;
using NTPAC.Reassembling.IP;
using NTPAC.Reassembling.TCP;
using NTPAC.Reassembling.UDP;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.ApplicationProtocolExport.L7ConversationProviders.Pcap
{
  public class PcapL7ConversationProvider : IL7ConversationProvider
  {
    private readonly IPcapLoader _pcapLoader;
    private readonly ILoggerFactory _loggerFactory;
    private readonly PcapL7ConversationProviderOptions _opts;
    
    public PcapL7ConversationProvider(IPcapLoader pcapLoader, ILoggerFactory loggerFactory, PcapL7ConversationProviderOptions opts)
    {
      this._pcapLoader = pcapLoader;
      this._loggerFactory = loggerFactory;
      this._opts = opts;
    }

    public Task<IEnumerable<IL7Conversation>> LoadAsync()
    {
      var l7Conversations = new List<IL7Conversation>();
      
      var trackers = new Dictionary<L3L4ConversationKey, L7ConversationTrackerBase>();
      
      foreach (var frame in this.GetFramesFromPcap(this._opts.PcapUri))
      {
        var tracker        = this.GetOrCreateL7ReassemblerForFrame(trackers, frame);
        var l7Conversation = tracker?.ProcessFrame(frame);
        if (l7Conversation != null)
        {
          l7Conversations.Add(l7Conversation);
        }
      }

      foreach (var tracker in trackers.Values)
      {
        l7Conversations.AddRange(tracker.Complete());
      }

      return Task.FromResult<IEnumerable<IL7Conversation>>(l7Conversations);
    }

    private L7ConversationTrackerBase GetOrCreateL7ReassemblerForFrame(Dictionary<L3L4ConversationKey, L7ConversationTrackerBase> trackers, Frame frame)
    {
      var conversationKey = new L3L4ConversationKey(frame);
      if (!trackers.TryGetValue(conversationKey, out var tracker))
      {
        switch (frame.IpProtocol)
        {
            case IPProtocolType.TCP:
              tracker = new TcpConversationTracker(frame.SourceEndPoint, frame.DestinationEndPoint);
              break;
            case IPProtocolType.UDP:
               tracker = new UdpConversationTracker(frame.SourceEndPoint, frame.DestinationEndPoint);
              break;
            default:
              return null;
        }
        trackers.Add(conversationKey, tracker);
      }
      return tracker;
    }
    
    private IEnumerable<ValueTuple<Packet, Int64>> GetPacketsFromPcap(Uri pcapUri)
    {
      using (this._pcapLoader)
      {
        RawCapture rawCapture;
        this._pcapLoader.Open(pcapUri);
        while ((rawCapture = this._pcapLoader.GetNextPacket()) != null)
        {
          var parsedPacket = Packet.ParsePacket(this._pcapLoader.LinkType, rawCapture.Data);
          yield return (parsedPacket, rawCapture.Timeval.Date.Ticks);
        }
      }
    }

    private IEnumerable<Frame> GetFramesFromPcap(Uri pcapUri)
    {
      var ipv4DefragmentationEngine = new Ipv4DefragmentationEngine(this._loggerFactory.CreateLogger<Ipv4DefragmentationEngine>());

      foreach (var (packet, timestampTicks) in this.GetPacketsFromPcap(pcapUri))
      {
        Frame frame;
        if (!(packet.PayloadPacket is IPPacket ipPacket))
        {
          continue;
        }

        if (ipPacket is IPv4Packet ipv4Packet && Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
        {
          var (isDefragmentationSuccessful, defragmentedIpv4Packet, fragments) =
            ipv4DefragmentationEngine.TryDefragmentFragments(FrameFactory.CreateFromIpPacket(ipv4Packet, timestampTicks));
          if (!isDefragmentationSuccessful)
          {
            continue;
          }

          frame = FrameFactory.CreateFromDefragmentedIpPacket(defragmentedIpv4Packet, fragments);
        }
        else
        {
          frame = FrameFactory.CreateFromPacket(packet, timestampTicks);
        }

        yield return frame;
      }
    }
  }
}
