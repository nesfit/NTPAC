using System.Text;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.Reassembling.TCP;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests
{
  public class PduStreamReaderTests : PduProvidersTestBase
  {
    public PduStreamReaderTests(ITestOutputHelper output) : base(output)
    {
    }
    
    [Fact]
    public void HttpReadLine_Breaked()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<TcpConversationTracker>(TestPcapFile.HttpTest0);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Breaked);
      var pduStreamReader = new PduStreamReader(stream, Encoding.ASCII);

      Assert.False(pduStreamReader.EndOfStream);
      Assert.Equal("GET / HTTP/1.1", pduStreamReader.ReadLine());
      Assert.Equal("Host: icanhazip.com", pduStreamReader.ReadLine());
      Assert.Equal("User-Agent: curl/7.54.0", pduStreamReader.ReadLine());
      Assert.Equal("Accept: */*", pduStreamReader.ReadLine());
      Assert.Equal("", pduStreamReader.ReadLine());
      Assert.True(pduStreamReader.EndOfStream);

      Assert.True(pduStreamReader.NewMessage());
      Assert.False(pduStreamReader.EndOfStream);

      var s = pduStreamReader.ReadToEnd();
      Assert.Equal(597, s.Length);
      Assert.StartsWith("HTTP/1.1 200 OK", s);
      Assert.EndsWith("147.229.14.29\n", s);
     
      Assert.True(pduStreamReader.EndOfStream);
    }
  }
}
