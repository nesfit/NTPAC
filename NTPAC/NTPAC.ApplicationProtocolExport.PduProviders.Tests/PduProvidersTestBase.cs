using NTPAC.Reassembling.Tests;
using Xunit.Abstractions;

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests
{
  public abstract class PduProvidersTestBase : ReassemblingTestBase
  {
    protected PduProvidersTestBase(ITestOutputHelper output) : base(output)
    {
    }
    
  }
}
