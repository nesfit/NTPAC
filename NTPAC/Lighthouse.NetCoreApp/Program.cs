using System;

namespace Lighthouse.NetCoreApp
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            var lighthouseService = new LighthouseService();
            lighthouseService.Start();
            Console.ReadLine();
            lighthouseService.StopAsync().Wait();
        }
    }
}
