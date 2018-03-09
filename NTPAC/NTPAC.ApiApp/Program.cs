using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace NTPAC.ApiApp
{
  public class Program
  {
    public static IWebHostBuilder CreateWebHostBuilder(String[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

    public static void Main(String[] args) { CreateWebHostBuilder(args).Build().Run(); }
  }
}
