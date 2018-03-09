using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.ApplicationProtocolExport.PduProviders.Tests.TestKaitaiStructs;
using NTPAC.Reassembling.UDP;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests
{
  public class PduKaitaiReaderTests : PduProvidersTestBase
  {
    public PduKaitaiReaderTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void ReadDns()
    {
      var l7Conversation  = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream          = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);
      var pduKaitaiReader = new PduKaitaiReader(stream);

      DnsPacket p;
      Assert.False(pduKaitaiReader.EndOfStream);
      p = pduKaitaiReader.ReadKaitaiStruct<DnsPacket>();
      Assert.Equal(0x09be, p.TransactionId);
      Assert.Single(p.Queries);
      Assert.True(pduKaitaiReader.NewMessage());
      p = pduKaitaiReader.ReadKaitaiStruct<DnsPacket>();
      Assert.Equal(0x09be, p.TransactionId);
      Assert.Single(p.Answers);
      Assert.False(pduKaitaiReader.NewMessage());
      Assert.True(pduKaitaiReader.EndOfStream);
    }
  }
}
