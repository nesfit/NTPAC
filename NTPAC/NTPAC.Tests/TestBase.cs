using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using NTPAC.Common.Interfaces;
using NTPAC.PcapLoader;
using SharpPcap;
using Xunit.Abstractions;

namespace NTPAC.Tests
{
  public class TestBase
  {
    protected readonly ServiceProvider _services;
    
    protected TestBase(ITestOutputHelper output)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
      this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
      serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
    }
    
    public IEnumerable<RawCapture> GetRawCapturesFromPcap(Uri pcapUri)
    {
      var pcapLoader = this._services.GetService<IPcapLoader>();
      using (pcapLoader)
      {
        RawCapture rawCapture;
        pcapLoader.Open(pcapUri);
        while ((rawCapture = pcapLoader.GetNextPacket()) != null)
        {
          yield return rawCapture;
        }
      }
    }

    public IEnumerable<RawCapture> GetRawCapturesFromPcap(String pcapFilename) =>
      this.GetRawCapturesFromPcap(TestPcapFile.GetTestPcapUri(pcapFilename));
  }
}
