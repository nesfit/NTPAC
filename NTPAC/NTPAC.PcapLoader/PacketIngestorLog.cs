using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using PacketDotNet;
using PacketDotNet.Interfaces;
using PacketDotNet.IP;
using PacketDotNet.Tcp;
using PacketDotNet.Udp;
using SharpPcap;

namespace NTPAC.PcapLoader
{
    public class PacketIngestorLog : IPacketIngestor
    {
        private readonly ILogger<PacketIngestorLog> _logger;
        private readonly IPcapLoader _pcapLoader;

        public PacketIngestorLog(IPcapLoader pcapLoader, ILoggerFactory loggerFactory)
        {
            this._pcapLoader = pcapLoader;
            this._logger     = loggerFactory.CreateLogger<PacketIngestorLog>();
        }


        public void OpenPcap(Uri uri) { Task.Run(async () => await this.OpenPcapAsync(uri)).Wait(); }

        public async Task OpenPcapAsync(Uri uri)
        {
            using (this._pcapLoader)
            {
                this._pcapLoader.Open(uri);

                RawCapture rawCapture = null;
                do
                {
                    rawCapture = this._pcapLoader.GetNextPacket();

                    // Parse the packet using PacketDotNet
                    if (rawCapture == null)
                    {
                        continue;
                    }

                    var capture = rawCapture;

                    var packet = await Task.Run(() => Packet.ParsePacket(this._pcapLoader.LinkType, capture.Data));
                    this.ProcessPacket(packet);
                } while (rawCapture != null);
            }
        }

        private void ProcessPacket(Packet packet)
        {
            var                    ipPacket        = packet?.PayloadPacket as IpPacket;
            ISourceDestinationPort transportPacket = null;

            switch (ipPacket?.PayloadPacket)
            {
                case TcpPacket tcpPacket:
                    transportPacket = tcpPacket;
                    break;
                case UdpPacket udpPacket:
                    transportPacket = udpPacket;
                    break;
            }

            this._logger.LogInformation($"{ipPacket?.SourceAddress}({transportPacket?.SourcePort}) <-> {ipPacket?.DestinationAddress}({transportPacket?.DestinationPort})");
        }
    }
}
