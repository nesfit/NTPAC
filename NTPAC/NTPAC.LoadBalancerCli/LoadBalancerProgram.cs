using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NTPAC.Common;
using NTPAC.Common.Interfaces;
using NTPAC.ConversationTracking.Actors;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Actors.Factories;
using NTPAC.LoadBalancer;
using NTPAC.LoadBalancer.Actors.Factories;
using NTPAC.LoadBalancer.Actors.Offline;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.PcapLoader;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.DevNull.Facades;
using NTPAC.Persistence.Generic.Facades;
using NTPAC.Persistence.Generic.Facades.Installers;
using NTPAC.Persistence.InMemory.Facades;
using NTPAC.Persistence.Interfaces;
using NTPAC.Persistence.Models;
using NTPAC.Persistence.Pcap.Facade;

[assembly: InternalsVisibleTo("NTPAC.LoadBalancer.Benchmark")]

namespace NTPAC.LoadBalancerCli
{
  public class LoadBalancerProgram
  {
    private static ILoadBalancerRunnerFactory _loadBalancerRunnerFactory;

    internal static async Task<Int32> RunOptionsAndReturnExitCodeAsync(LoadBalancerCliOptions opts)
    {
      if (opts.Uri.IsFile && !File.Exists(opts.Uri.AbsolutePath))
      {
        Console.Error.WriteLine($"{nameof(LoadBalancerProgram)} {opts.Uri}: No such file");
        return 1;
      }
      
      ConfigureServices(opts);
      
      var sw = new Stopwatch();
      sw.Start();

      var runner = _loadBalancerRunnerFactory.CreateInstance();

      var processingResult = await runner.Run(opts.Uri).ConfigureAwait(false);

      Console.WriteLine(processingResult.ToString());
      
      Console.WriteLine(
        $"{nameof(LoadBalancerProgram)} has taken {sw.Elapsed} to process {opts.Uri.AbsolutePath} in {(opts.Offline ? "offline" : "online")} mode");

      return 0;
    }

    private static Boolean CheckCmdOptions(LoadBalancerCliOptions cliOptions)
    {
      if (!cliOptions.Offline &&
          (cliOptions.CassandraRepository || cliOptions.DevnullRepository || cliOptions.InMemoryRepository))
      {
        Console.WriteLine("Repository cannot be defined in online mode!");
        return false;
      }

      return true;
    }

    private static void ConfigureServices(LoadBalancerCliOptions opts)
    {
      IServiceCollection services = new ServiceCollection();
      services.AddLogging();
      services.AddSingleton(services);
      services.AddSingleton(provider => provider);
      services.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();

      services.AddSingleton<LoadBalancerRunner>();
      services.AddSingleton<IAkkaSettings>(new AkkaSettings {IsDebug = opts.IsDebug});

      if (opts.Uri.Scheme == "rpcap")
      {
        services.AddSingleton<ICaptureDeviceFactory, CaptureLiveDeviceFactory>();
      }
      else
      {
        services.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
      }

      services.AddSingleton<ILoadBalancerRunner, LoadBalancerRunner>();
      services.AddSingleton<ILoadBalancerRunnerFactory, LoadBalancerRunnerFactory>();
      services.AddSingleton<IBatchLoader, BatchLoader>();
      services.AddSingleton<IBatchSender, BatchSender>();
    
      ActorsServiceInstaller.Install(services);
      FacadesServiceInstaller.Install(services);
      
      if (opts.Offline)
      {
        services.AddSingleton<IPacketIngestor, OfflineLoadBalancer>();
        services.AddSingleton<IOfflineLoadBalancerActorFactory, OfflineLoadBalancerActorFactory>();
      }
      else
      {
        services.AddSingleton<IPacketIngestor, OnlineLoadBalancer>();
        services.AddSingleton<IClusterSettings, ClusterSettings>(
          provider => new ClusterSettings
                      {
                        ClusterNodeHostname     = opts.ClusterNodeHostname,
                        ClusterNodePort         = opts.ClusterNodePort,
                        ClusterSeedNodeHostname = opts.ClusterSeedNodeHostname
                      });
        services.AddSingleton<IClusterMember, OnlineLoadBalancer>();
        services.AddSingleton<IOnlineLoadBalancerActorFactory, OnlineLoadBalancerActorFactory>();
      }

      if (opts.DevnullRepository)
      {
        DevNullServiceInstaller.Install(services);
      }
      else if (opts.CassandraRepository)
      {
        CassandraServiceInstaller.Install(services, opts.CassandraKeyspace, opts.CassandraContactPoint);
      }
      // Not specified or InMemoryRepository selected
      else
      {
        InMemoryServiceInstaller.Install(services);
      }
      
      if (opts.OutPcapDirectory != null)
      {
        services.AddSingleton<IPcapFacadeConfiguration>(new PcapFacadeConfiguration
                                                                 {
                                                                   BaseDirectory = opts.OutPcapDirectory
                                                                 });
        services.AddSingleton<IPcapFacade, PcapFacade>();
      }
      else
      {
        services.AddSingleton<IPcapFacade, PcapDevnullFacade>();
      }

      services.AddLogging(builder => builder.AddConsole());

      var serviceProvider = services.BuildServiceProvider();
      _loadBalancerRunnerFactory = serviceProvider.GetService<ILoadBalancerRunnerFactory>();
    }

    private static async Task<Int32> Main(String[] args)
    {
      LoadBalancerCliOptions cliOptions = null;
      Parser.Default.ParseArguments<LoadBalancerCliOptions>(args).WithParsed(opt => cliOptions = opt);

      if (cliOptions == null || !CheckCmdOptions(cliOptions))
      {
        return 1;
      }

      return await RunOptionsAndReturnExitCodeAsync(cliOptions).ConfigureAwait(false);
    }
  }
}
