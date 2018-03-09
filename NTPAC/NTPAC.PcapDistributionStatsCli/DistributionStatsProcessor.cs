using System;
using System.Collections.Generic;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.RawPacketParser;
using NTPAC.Messages.RawPacket;
using NTPAC.PcapDistributionStatsCli.Interfaces;
using NTPAC.PcapDistributionStatsCli.Models;
using SharpPcap;

namespace NTPAC.PcapDistributionStatsCli
{
  public class DistributionStatsProcessor : IDistributionStatsProcessor
  {
    private readonly IPcapLoader _pcapLoader;

    public DistributionStatsProcessor(IPcapLoader pcapLoader)
    {
      this._pcapLoader = pcapLoader;
    }
    
    public DistributionStatsReport Process(Uri pcapUri, Int32 numberOfShards)
    {
      var shardsOccurence = new UInt64[numberOfShards];
      var shardsSize = new UInt64[numberOfShards];
      
      foreach (var rawPacket in this.LoadRawPacketsFromPcap(pcapUri))
      {
        if (!RawPacketEntityIdSetter.SetEntityIdForRawPacket(rawPacket, numberOfShards))
        {
          continue;
        }

        shardsOccurence[rawPacket.EntityId]++;
        shardsSize[rawPacket.EntityId] += (UInt64) rawPacket.RawPacketData.Length;
      }
      
      var report = new DistributionStatsReport
             {
               ShardsOccurence = shardsOccurence,
               ShardsSize = shardsSize
             };
      return report;
    }

    private IEnumerable<RawPacket> LoadRawPacketsFromPcap(Uri pcapUri)
    {
      using (this._pcapLoader)
      {
        this._pcapLoader.Open(pcapUri);
        RawCapture rawCapture;
        while ((rawCapture = this._pcapLoader.GetNextPacket()) != null)
        {
          var rawPacket = new RawPacket(rawCapture);
          yield return rawPacket;
        }
      }
    }    
  }
}
