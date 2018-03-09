using System;
using CommandLine;

namespace NTPAC.PcapLoader.Benchmark
{
  public class CliOptions
  {
    [Option('d', "directory", Required = false, HelpText = "Test pcap files base directory", Default = "/pcap")]
    public String BaseDirectory { get; set; }

    [Option('p', "pcap", Required = false, HelpText = "Test pcap filename inside base directory", Default = "sec6net-1gb.pcap")]
    public String TestPcapFilename { get; set; }
  }
}
