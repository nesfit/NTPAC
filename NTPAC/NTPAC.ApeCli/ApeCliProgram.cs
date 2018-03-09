using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.Serializers;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ApplicationProtocolExport.L7ConversationProviders.Pcap;
using NTPAC.ApplicationProtocolExport.L7ConversationProviders.Repository;
using NTPAC.Common.Interfaces;
using NTPAC.PcapLoader;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.Generic.Facades;
using NTPAC.Persistence.Interfaces;
using SnooperDNS;
using SnooperHTTP;
using SnooperTLS;

namespace NTPAC.ApeCli
{
  public static class ApeCliProgram
  {
    private static ServiceProvider _services;
    
    private static void ConfigureServices(ApeCliOptions opts)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      
      if (opts.PcapProviderUri != null)
      {
        serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
        serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
        serviceCollection.AddSingleton<IL7ConversationProvider, PcapL7ConversationProvider>();
        serviceCollection.AddSingleton(new PcapL7ConversationProviderOptions { PcapUri = opts.PcapProviderUri});
      }
      // Setup for Cassandra repository 
      else
      {
        serviceCollection.AddSingleton<IL7ConversationProvider, RepositoryL7ConversationProvider>();
        
        var dynamicExpressoInterpreter = new DynamicExpresso.Interpreter();
        Expression<Func <IL7ConversationEntity, Boolean >> l7ConversationFilterPredicate = null;
        
        if (opts.L7ConversationFilter != null)
        {
          l7ConversationFilterPredicate = dynamicExpressoInterpreter
            .ParseAsExpression<Func<IL7ConversationEntity, Boolean>>(opts.L7ConversationFilter, "l7c");
        }
        serviceCollection.AddSingleton(new RepositoryL7ConversationProviderOptions
                                       {
                                         L7ConversationFilterPredicate = l7ConversationFilterPredicate
                                       });
      }

      if (opts.PrintSnooperExports == false)
      {
        serviceCollection.AddSingleton<ISnooperExportFacade, SnooperExportFacade>();   
      }
      
      serviceCollection.AddSingleton<ICaptureFacade, CaptureFacade>();
      serviceCollection.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();
      serviceCollection.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();
      CassandraServiceInstaller.Install(serviceCollection, opts.CassandraKeyspace, opts.CassandraContactPoint);
      
      _services = serviceCollection.BuildServiceProvider();
    }

    private static async Task StoreSnooperExports(ISnooperExportFacade snooperExportFacade, IEnumerable<SnooperExportCollection> snoopersExportsCollections)
    {
      foreach (var snooperExportCollection in snoopersExportsCollections)
      {
        await snooperExportFacade.InsertAsync(snooperExportCollection);
      }
    }

    private static void PrintSnooperExports(IEnumerable<SnooperExportCollection> snoopersExportsCollections)
    {
      Console.WriteLine(SnooperExportCollectionsPrettyJsonSerializer.Serialize(snoopersExportsCollections));
    }

    private static SnooperRunner CreateSnooperManager()
    {
      var snooperManager = new SnooperRunner();
      
      snooperManager.RegisterSnooper<SnooperDns>();
      snooperManager.RegisterSnooper<SnooperHttp>();
      snooperManager.RegisterSnooper<SnooperTls>();

      return snooperManager;
    }

    public static async Task Main(String[] args)
    {
      ApeCliOptions cliOpts = null;
      Parser.Default.ParseArguments<ApeCliOptions>(args).WithParsed(parsedCliOpts => cliOpts = parsedCliOpts);
      if (cliOpts == null)
      {
        return;
      }
      
      ConfigureServices(cliOpts);

      var snooperManager = CreateSnooperManager();
     
      var l7Conversations = await _services.GetRequiredService<IL7ConversationProvider>().LoadAsync().ConfigureAwait(false);
      var snoopersExportsCollections = snooperManager.Run(l7Conversations).ToList();

      var snoopersExportFacade = _services.GetService<ISnooperExportFacade>();
      if (snoopersExportFacade != null)
      {
        await StoreSnooperExports(snoopersExportFacade, snoopersExportsCollections);
      }
      else
      {
        PrintSnooperExports(snoopersExportsCollections);
      }
    }
  }
}
