using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.LoadBalancer.Actors.Online;
using NTPAC.LoadBalancer.RawPacketParser;
using NTPAC.Messages.RawPacket;
using NTPAC.Tests;
using PacketDotNet;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.LoadBalancer.Tests
{
  public class RawPacketSignatureParserTests : TestBase
  {
    public RawPacketSignatureParserTests(ITestOutputHelper output) : base(output)
    {
    }
    
    private IList<RawPacket> GetRawPacketsFromPcap(String pcapFileName)
      => this.GetRawCapturesFromPcap(pcapFileName).Select(rawCapture => new RawPacket(rawCapture)).ToList();
    
    [Fact]
    public void SignatureUdp()
    {
      var rawPackets= this.GetRawPacketsFromPcap(TestPcapFile.Dns1);
      var rp = rawPackets[0];
      var success =
        RawPacketSignatureParser.ExtractRawPacketSignature(rp, out var srcIp, out var dstIp, out var srcPort, out var dstPort,
                                                          out var ipProto, out var fragmentSignature);
      
      Assert.True(success);
      Assert.Null(fragmentSignature);
      Assert.Equal(IPAddress.Parse("147.229.176.17"), srcIp);
      Assert.Equal(IPAddress.Parse("147.229.8.12"), dstIp);
      Assert.Equal(60416, srcPort);
      Assert.Equal(53, dstPort);
      Assert.Equal(IPProtocolType.UDP, ipProto);
    }
    
    [Fact]
    public void SignatureUdpIpv6()
    {
      var rawPackets = this.GetRawPacketsFromPcap(TestPcapFile.DnsIpv6);
      var rp         = rawPackets[0];
      var success =
        RawPacketSignatureParser.ExtractRawPacketSignature(rp, out var srcIp, out var dstIp, out var srcPort, out var dstPort,
                                                          out var ipProto, out var fragmentSignature);
      
      Assert.True(success);
      Assert.Null(fragmentSignature);
      Assert.Equal(IPAddress.Parse("2001:67c:1220:80c:e545:d8e7:663d:8575"), srcIp);
      Assert.Equal(IPAddress.Parse("2001:67c:1220:809::93e5:92b"), dstIp);
      Assert.Equal(64753, srcPort);
      Assert.Equal(53, dstPort);
      Assert.Equal(IPProtocolType.UDP, ipProto);
    }

    [Fact]
    public void SignatureHttp()
    {
      var rawPackets = this.GetRawPacketsFromPcap(TestPcapFile.HttpTest0);
      var rp         = rawPackets[0];
      var success =
        RawPacketSignatureParser.ExtractRawPacketSignature(rp, out var srcIp, out var dstIp, out var srcPort, out var dstPort,
                                                          out var ipProto, out var fragmentSignature);
      
      Assert.True(success);
      Assert.Null(fragmentSignature);
      Assert.Equal(IPAddress.Parse("192.168.1.216"), srcIp);
      Assert.Equal(IPAddress.Parse("104.20.16.242"), dstIp);
      Assert.Equal(52720, srcPort);
      Assert.Equal(80, dstPort);
      Assert.Equal(IPProtocolType.TCP, ipProto);
    }

    [Fact]
    public void EntityIdsRange()
    {
      const Int32 maxShards = 20;
      foreach (var rawPacket in GetRawPacketsFromPcap(TestPcapFile.VariousBigger))
      {
        if (!RawPacketEntityIdSetter.SetEntityIdForRawPacket(rawPacket, maxShards))
        {
          continue;
        }

        var entityId = rawPacket.EntityId;
        Assert.True(entityId >= 0);
        Assert.True(entityId < maxShards);
      }
    }
  }
}
