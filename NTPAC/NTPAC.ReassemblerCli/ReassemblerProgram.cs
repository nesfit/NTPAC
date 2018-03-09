using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NTPAC.Common;
using NTPAC.Common.Interfaces;
using NTPAC.ConversationTracking.Actors;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Actors.Factories;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.DevNull.Facades;
using NTPAC.Persistence.Generic.Facades;
using NTPAC.Persistence.Generic.Facades.Installers;
using NTPAC.Persistence.InMemory.Facades;
using NTPAC.Persistence.Interfaces;
using NTPAC.Persistence.Models;
using NTPAC.Persistence.Pcap.Facade;

namespace NTPAC.ReassemblerCli
{
  internal class ReassemblerProgram
  {
    private static void ConfigureServices(IServiceCollection services, ReassemblerCliOptions opts)
    {
      services.AddLogging();

      services.AddSingleton<IClusterSettings, ClusterSettings>(
        provider => new ClusterSettings
                    {
                      ClusterNodeHostname     = opts.ClusterNodeHostname,
                      ClusterNodePort         = opts.ClusterNodePort,
                      ClusterSeedNodeHostname = opts.ClusterSeedNodeHostname
                    });

      services.AddSingleton<IHostedService, ReassemblerService>();

      ActorsServiceInstaller.Install(services);
      FacadesServiceInstaller.Install(services);

      if (opts.DevnullRepository)
      {
        DevNullServiceInstaller.Install(services);
      }
      else if (opts.CassandraRepository)
      {
        CassandraServiceInstaller.Install(services, opts.CassandraKeyspace, opts.CassandraContactPoint);
      }
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
    }

    private static async Task<Int32> Main(String[] args)
    {
      ReassemblerCliOptions cliOptions = null;
      Parser.Default.ParseArguments<ReassemblerCliOptions>(args).WithParsed(options => cliOptions = options);
      if (cliOptions == null)
      {
        return 1;
      }
      
      var hostBuilder = new HostBuilder().ConfigureServices((hostContext, services) =>
      {
       ConfigureServices(services, cliOptions);
      });
      await hostBuilder.RunConsoleAsync().ConfigureAwait(false);

      return 0;
    }
  }
}
