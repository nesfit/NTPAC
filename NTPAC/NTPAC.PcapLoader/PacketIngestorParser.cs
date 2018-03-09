using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.PcapLoader
{
    public class PacketIngestorParser : IPacketIngestor
    {
        private readonly ILogger<PacketIngestorParser> _logger;
        private readonly IPcapLoader _pcapLoader;

        public PacketIngestorParser(IPcapLoader pcapLoader, ILoggerFactory loggerFactory)
        {
            this._pcapLoader = pcapLoader;
            this._logger     = loggerFactory.CreateLogger<PacketIngestorParser>();
        }

        private void ProcessPacket(Packet packet)
        {
            var             ipPacket        = packet?.PayloadPacket as IPPacket;
            TransportPacket transportPacket = null;

            switch (ipPacket?.PayloadPacket)
            {
                case TcpPacket tcpPacket:
                    transportPacket = tcpPacket;
                    break;
                case UdpPacket udpPacket:
                    transportPacket = udpPacket;
                    break;
            }

            var loggingMessage =
                $"{ipPacket?.SourceAddress}({transportPacket?.SourcePort}) <-> {ipPacket?.DestinationAddress}({transportPacket?.DestinationPort})";
            this._logger.LogInformation(loggingMessage);
        }


        public IProcessingResult OpenCapture(Uri uri)
        {
            using (this._pcapLoader)
            {
                this._pcapLoader.Open(uri);

                RawCapture rawCapture;
                do
                {
                    rawCapture = this._pcapLoader.GetNextPacket();

                    // Parse the packet using PacketDotNet
                    if (rawCapture == null)
                    {
                        continue;
                    }

                    var capture = rawCapture;
                    var packet  = Packet.ParsePacket(this._pcapLoader.LinkType, capture.Data);
                    this.ProcessPacket(packet);
                } while (rawCapture != null);
            }

            return null;
        }

        public Task<IProcessingResult> OpenCaptureAsync(Uri uri)
        {
            var parseAndLogPacket = new ActionBlock<RawCapture>(capture =>
            {
                var packet = Packet.ParsePacket(this._pcapLoader.LinkType, capture.Data);
                this.ProcessPacket(packet);
            });

            return Task.Run(() =>
            {
                using (this._pcapLoader)
                {
                    this._pcapLoader.Open(uri);

                    RawCapture rawCapture;
                    do
                    {
                        rawCapture = this._pcapLoader.GetNextPacket();

                        // Parse the packet using PacketDotNet
                        if (rawCapture == null)
                        {
                            continue;
                        }

                        parseAndLogPacket.Post(rawCapture);
                    } while (rawCapture != null);
                }

                parseAndLogPacket.Complete();
                return (IProcessingResult) null;
            });
        }
    }
}
