using System;
using NTPAC.PcapDistributionStatsCli.Models;

namespace NTPAC.PcapDistributionStatsCli.Interfaces
{
  public interface IDistributionStatsProcessor
  {
    DistributionStatsReport Process(Uri pcapUri, Int32 numberOfShards);
  }
}
