using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NTPAC.Common.Extensions;
using NTPAC.Common.Interfaces;

namespace NTPAC.LoadBalancerCli
{
    public class LoadBalancerProgram
    {
        private static ServiceProvider _services;

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
            serviceCollection.AddSingleton<IPacketIngestor, LoadBalancer.LoadBalancer>();
        }

        private static void Die(String msg, Int32 exitCode = 1)
        {
            Console.Error.WriteLine(msg);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        private static void Main(String[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _services = serviceCollection.BuildServiceProvider();
            _services.GetService<ILoggerFactory>().AddProvider(new ConsoleLoggerProvider((s, level) => level >= LogLevel.Debug, true));

            String pcapFilePath;

            if (args.Any())
            {
                pcapFilePath = args[0];
            }
            else
            {
                //pcapFilePath = @"c:\big.pcap";
                pcapFilePath = @"c:\sec6net.pcap";
                //pcapFilePath = @"c:\small.pcap";
                //pcapFilePath = @"/Users/vilco/Desktop/various.pcapng";
                //pcapFilePath = @"/Users/vilco/Desktop/isa-http.pcapng";
            }

            var pcap = new FileInfo(pcapFilePath);
            try
            {
                var loadBalancer = _services.GetService<IPacketIngestor>();

                Task.Run(async () => await loadBalancer.OpenPcapAsync(pcap.ToUri())).Wait();
            }
            catch (Exception e)
            {
                Die(e.ToString());
            }

            WaitForPressedKey();
        }

        private static void WaitForPressedKey()
        {
            if (Console.IsInputRedirected)
            {
                return;
            }

            Console.WriteLine("Press ANY key to continue...");
            Console.ReadKey();
        }
    }
}
