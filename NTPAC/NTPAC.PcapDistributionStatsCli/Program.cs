using System;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Common.Interfaces;
using NTPAC.PcapDistributionStatsCli.Interfaces;
using NTPAC.PcapDistributionStatsCli.Models;
using NTPAC.PcapLoader;

namespace NTPAC.PcapDistributionStatsCli
{
  class Program
  {
    private static void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();
      services.AddSingleton(services);
      services.AddSingleton(provider => provider);
      services.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
      services.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
      services.AddSingleton<IDistributionStatsProcessor, DistributionStatsProcessor>();
    }

    private static void PrintDistributionStatsReport(DistributionStatsReport report)
    {
      Console.WriteLine("Occurence:");
      HistogramCliPrinter.Print(report.ShardsOccurence);
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("Payload size:");
      HistogramCliPrinter.Print(report.ShardsSize);
    } 

    private static void Main(String[] args)
    {
      Parser.Default.ParseArguments<Opts>(args).WithParsed(Main).WithNotParsed(_ => Environment.Exit(1));
    }

    private static void Main(Opts opts)
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      var services = serviceCollection.BuildServiceProvider();

      var processor = services.GetRequiredService<IDistributionStatsProcessor>();
      var report = processor.Process(opts.PcapUri, opts.Shards);
      
      Console.WriteLine($"PCAP: {opts.PcapUri}");
      PrintDistributionStatsReport(report);
    }
  }
}
