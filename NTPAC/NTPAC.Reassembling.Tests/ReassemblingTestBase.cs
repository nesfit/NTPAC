using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTPAC.ConversationTracking.Factories;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.IP;
using NTPAC.Tests;
using PacketDotNet;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public abstract class ReassemblingTestBase : TestBase
  {
    protected ReassemblingTestBase(ITestOutputHelper output) : base(output)
    {
    }

    protected IEnumerable<Frame> GetFramesFromPcap(String pcapFileName, Boolean defragment = false)
    {
      return defragment ? this.GetFramesFromPcapAndDefragment(pcapFileName) : this.GetFramesFromPcapWithoutDefragmentation(pcapFileName);
    }

    protected IEnumerable<Frame> GetFramesFromPcapWithoutDefragmentation(String pcapFileName)
    {
      var frames     = new List<Frame>();
      foreach (var (packet, timestampTicks) in this.GetPacketsFromPcap(pcapFileName))
      {
        var frame = FrameFactory.CreateFromPacket(packet, timestampTicks);
        frames.Add(frame);
      }
      return frames;
    }

    protected IEnumerable<Frame> GetFramesFromPcapAndDefragment(String pcapFileName)
    {
      var ipv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(this._services.GetService<ILoggerFactory>().CreateLogger<Ipv4DefragmentationEngine>());
      var frames     = new List<Frame>();

      foreach (var (packet, timestampTicks) in this.GetPacketsFromPcap(pcapFileName))
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

        frames.Add(frame);
      }

      return frames;
    }

    public IEnumerable<ValueTuple<Packet, Int64>> GetPacketsFromPcap(String pcapFileName)
    {
      return this.GetRawCapturesFromPcap(pcapFileName).Select(rawCapture =>
      {
        var parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
        return (parsedPacket, rawCapture.Timeval.Date.Ticks);
      });
    }
   
    protected IReadOnlyList<IL7Conversation> ReassembleL7Conversations<T>(String pcapFileName) where T:L7ConversationTrackerBase
    {
      var framesEnumerable = this.GetFramesFromPcap(pcapFileName, true);

      var frames = framesEnumerable as Frame[] ?? framesEnumerable.ToArray();
      var originatorFrame = frames.First();
      
      var reassembler = (T) Activator.CreateInstance(typeof(T), originatorFrame.SourceEndPoint, originatorFrame.DestinationEndPoint);
      
      var l7Conversations = frames.Select(frame => reassembler.ProcessFrame(frame))
                                  .Where(reassembledL7Conversation => reassembledL7Conversation != null)
                                  .ToList();
      l7Conversations.AddRange(reassembler.Complete());
      return l7Conversations;
    }

    protected IL7Conversation ReassembleSingleL7Conversation<T>(String pcapFileName) where T : L7ConversationTrackerBase
    {
      var l7Conversations = this.ReassembleL7Conversations<T>(pcapFileName);
      Assert.Single(l7Conversations);
      return l7Conversations.First();
    }
  }
}
