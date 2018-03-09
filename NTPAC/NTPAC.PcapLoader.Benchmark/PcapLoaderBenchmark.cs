using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Common.Extensions;
using NTPAC.Common.Interfaces;

namespace NTPAC.PcapLoader.Benchmark
{
    [SimpleJob(RunStrategy.Throughput, 1, 1, 2, 1)]
    public class PcapLoaderBenchmark
    {
        public static readonly String BaseDirectoryFullPathEnvName = "BASE_DIRECTORY_FULL_PATH";
        public static readonly String TestPcapFilenameEnvName = "TEST_PCAP_FILENAME";

        private ServiceProvider _services;
        public static String BaseDirectoryFullPath { get; } = Environment.GetEnvironmentVariable(BaseDirectoryFullPathEnvName);
        public static String TestPcapFilename { get; } = Environment.GetEnvironmentVariable(TestPcapFilenameEnvName);

        [Benchmark]
        public void BenchmarkSync() { this.Run(TestPcapFilename); }

        [Benchmark]
        public async Task BenchmarkAsync() { await this.RunAsync(TestPcapFilename).ConfigureAwait(false); }
       
        [GlobalSetup]
        public void Setup()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            this._services = serviceCollection.BuildServiceProvider();
            //this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton(serviceCollection);
            serviceCollection.AddTransient<IPcapLoader, PcapLoader>();
            serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
            serviceCollection.AddTransient<IPacketIngestor, PacketIngestorParser>();
        }

        private void Run(String pcapFileName)
        {
            var fileInfo     = new FileInfo(Path.Combine(BaseDirectoryFullPath, pcapFileName));
            var pcapIngestor = this._services.GetRequiredService<IPacketIngestor>();

            pcapIngestor.OpenCapture(fileInfo.ToUri());
        }

        private async Task RunAsync(String pcapFileName)
        {
            var fileInfo     = new FileInfo(Path.Combine(BaseDirectoryFullPath, pcapFileName));
            var pcapIngestor = this._services.GetRequiredService<IPacketIngestor>();

            await Task.Run(async () => await pcapIngestor.OpenCaptureAsync(fileInfo.ToUri()).ConfigureAwait(false))
                      .ConfigureAwait(false);
        }
    }
}
