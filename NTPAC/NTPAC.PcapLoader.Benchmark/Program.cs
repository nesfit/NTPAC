using System;
using System.IO;
using BenchmarkDotNet.Running;
using CommandLine;

namespace NTPAC.PcapLoader.Benchmark
{
  //  $ dotnet.exe .\bin\Release\netcoreapp2.1\NTPAC.PcapLoader.Benchmark.dll
  internal class Program
  {
    private static void Main(String[] args)
    {
      Parser.Default.ParseArguments<CliOptions>(args).WithParsed(MainWithOpts);
    }

    private static void MainWithOpts(CliOptions opts)
    {
      Console.WriteLine($"Starting benchmark with {Path.Combine(opts.BaseDirectory, opts.TestPcapFilename)}");
      
      Environment.SetEnvironmentVariable(PcapLoaderBenchmark.BaseDirectoryFullPathEnvName, opts.BaseDirectory);
      Environment.SetEnvironmentVariable(PcapLoaderBenchmark.TestPcapFilenameEnvName,  opts.TestPcapFilename);
      
      BenchmarkRunner.Run<PcapLoaderBenchmark>();
    }
  }
}
