using System;
using CommandLine;
using NTPAC.Common;

namespace NTPAC.PcapDistributionStatsCli
{
  public class Opts
  {
    [Option('s', "shards", Default = 10, HelpText = "Number of shards")]
    public Int32 Shards { get; set; }
    
    [Value(0, Required = true, HelpText = "Uri for capture file")]
    public RelativeFileUri PcapUri { get; set; }
  }
}
