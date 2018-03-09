using System;
using System.IO;
using NTPAC.Common.Extensions;

namespace NTPAC.Tests
{
  public static class TestPcapFile
  {
    public const String BaseDir = @"..\..\..\..\TestingData";
 
    public static Uri GetTestPcapUri(String pcapFileName)
    {
      var pcapFilePath = Path.Combine(BaseDir, pcapFileName);
      var fileInfo = new FileInfo(pcapFilePath);
      return fileInfo.ToUri();
    }

    #region Test pcap files
    public const String Dns1 = "dns_1.pcap";
    public const String DnsIpv6 = "dns_ipv6.pcap";
    public const String HttpTest0 = "httpTest0.pcap";
    public const String HttpTest1 = "httpTest1.pcap";
    public const String HttpTest2 = "httpTest2.pcap";
    public const String IsaHttpDns = "isa-http_udp.pcap";
    public const String IsaHttpFragment = "isa-http_fragment.pcap";
    public const String IsaHttpKeepAlive1Dst1 = "isa-http_keepalive1_dst1.pcap";
    public const String IsaHttpRetransmission = "isa-http_retransmission.pcap";
    public const String IsaHttpReusedPortsAndKeepalives = "isa-http_reusedPortsAndKeepalives.pcap";
    public const String Sec6NetFTCP49795 = "sec6net-f_tcp49795.pcap";
    public const String TcpKeepAlive = "tcp_keepalive.pcap";
    public const String TcpMissingFrames = "tcp_missing_frames.pcap";
    public const String TcpMoreFrames = "tcp_more_frames.pcap";
    public const String TcpMoreFramesUniA = "tcp_more_frames_uniA.pcap";
    public const String TcpMoreFramesUniB = "tcp_more_frames_uniB.pcap";
    public const String TcpOutOfOrder = "tcp_out_of_order.pcap";
    public const String TcpTwoFrames = "tcp_two_frames.pcap";
    public const String TcpInversedPorts = "tcp_inversed_ports.pcap";
    public const String TcpWithRstReason = "tcp_rst_with_reason.pcap";
    public const String VariousBigger = "various_bigger.pcap";
    #endregion
  }
}
