using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using SharpPcap;

namespace NTPAC.PcapLoader
{
  public class PacketIngestorDiscarder : IPacketIngestor
  {
      private readonly ILogger<PacketIngestorDiscarder> _logger;
        private readonly IPcapLoader _pcapLoader;

        public PacketIngestorDiscarder(IPcapLoader pcapLoader, ILoggerFactory loggerFactory)
        {
            this._pcapLoader = pcapLoader;
            this._logger     = loggerFactory.CreateLogger<PacketIngestorDiscarder>();
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
                } while (rawCapture != null);
            }

            return null;
        }

        public Task<IProcessingResult> OpenCaptureAsync(Uri uri)
        {
            return Task.FromResult<IProcessingResult>(null);
            
//            var parseAndLogPacket = new ActionBlock<RawCapture>(capture =>
//            {
//                var packet = Packet.ParsePacket(this._pcapLoader.LinkType, capture.Data);
//            });
//
//            return Task.Run(() =>
//            {
//                using (this._pcapLoader)
//                {
//                    this._pcapLoader.Open(uri);
//
//                    RawCapture rawCapture;
//                    do
//                    {
//                        rawCapture = this._pcapLoader.GetNextPacket();
//
//                        // Parse the packet using PacketDotNet
//                        if (rawCapture == null)
//                        {
//                            continue;
//                        }
//
//                        parseAndLogPacket.Post(rawCapture);
//                    } while (rawCapture != null);
//                }
//
//                parseAndLogPacket.Complete();
//                return (IProcessingResult) null;
//            });
        }
  }
}
