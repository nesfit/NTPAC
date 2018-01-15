using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using NTPAC.Common.Extensions;
using NTPAC.Common.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.PcapLoader.Tests
{
    public class PcapLoaderTest
    {
        private readonly IServiceProvider _services;

        public PcapLoaderTest(ITestOutputHelper output)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            this._services = serviceCollection.BuildServiceProvider();
            this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
        }

        //[Fact(Skip = "provide big.pcap manually")]
        [Fact]
        public void Big()
        {
            var loader = this._services.GetService<IPacketIngestor>();

            var fileInfo = new FileInfo(@"C:\big.pcap");
            loader.OpenPcapAsync(fileInfo.ToUri());
        }

        [Fact]
        public void Small()
        {
            var loader = this._services.GetService<IPacketIngestor>();

            var fileInfo = new FileInfo("small.pcap");
            loader.OpenPcapAsync(fileInfo.ToUri());
        }


        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IPcapLoader, PcapLoader>();
            serviceCollection.AddSingleton<IPacketIngestor, PacketIngestorLog>();
        }
    }
}
