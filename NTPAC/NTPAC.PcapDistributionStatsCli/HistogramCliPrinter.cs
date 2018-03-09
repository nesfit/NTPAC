using System;
using System.Collections.Generic;
using System.Linq;

namespace NTPAC.PcapDistributionStatsCli
{
  public static class HistogramCliPrinter
  {
    public static void Print(IReadOnlyList<UInt64> sourceArray, Int32 width = 50)
    {
      var max = (Single) sourceArray.Max();
      for (var i = 0; i < sourceArray.Count; i++)
      {
        var val = sourceArray[i];
        var points = (Int32) Math.Round((val / max) * width);
        Console.WriteLine($"{i} ({val.ToString()})".PadRight(15) + $"| {new String('*', points).PadRight(width)} |");
      }
    }
  }
}
