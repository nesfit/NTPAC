using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using NTPAC.Common.Interfaces;
using NTPAC.Common.Models;
using PacketDotNet;
using SharpPcap;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
    public abstract class ReassemblingTestBase
    {
        protected static readonly String TestPcapsDir = "/Users/vilco/Desktop/testPcaps";
        
        private readonly ServiceProvider _services;
        
        protected ReassemblingTestBase(ITestOutputHelper output)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            this._services = serviceCollection.BuildServiceProvider();
            this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
        }
        
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
        }

        protected IEnumerable<Frame> GetFramesFromPcap(String pcapFileName) => this.GetFramesFromPcap(new Uri(TestPcapPath(pcapFileName)));
        
        protected IEnumerable<Frame> GetFramesFromPcap(Uri pcapUri)
        {   
            var frames = new List<Frame>();
            var pcapLoader = this._services.GetService<IPcapLoader>();
            using (pcapLoader)
            {
                RawCapture rawCapture;
                pcapLoader.Open(pcapUri);
                while ((rawCapture = pcapLoader.GetNextPacket()) != null )
                {
                    var packet = Packet.ParsePacket(pcapLoader.LinkType, rawCapture.Data);
                    var frame = new Frame(packet, rawCapture.Timeval.Date.Ticks);
                    frames.Add(frame);
                }
            }
            return frames;
        }

        private static String TestPcapPath(String pcapFileName) => Path.Combine(TestPcapsDir, pcapFileName);
    }
}
