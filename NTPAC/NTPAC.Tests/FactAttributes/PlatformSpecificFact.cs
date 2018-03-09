using System;
using Xunit;

namespace NTPAC.Tests.FactAttributes
{
  public class PlatformSpecificFact : FactAttribute
  {
    public PlatformSpecificFact(PlatformID targetPlatformId)
    {
      var currentPlatformId = Environment.OSVersion.Platform;
      if (currentPlatformId != targetPlatformId)
      {
        Skip = $"Skipped on {currentPlatformId.ToString()}";
      }
    }
  }
}
