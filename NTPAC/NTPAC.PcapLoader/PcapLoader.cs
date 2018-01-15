using System;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NTPAC.PcapLoader
{
    public class PcapLoader : IDisposable, IPcapLoader
    {
        private readonly ILogger _logger;
        private CaptureFileReaderDevice _captureDevice;
        private Boolean _disposed;
        private LinkLayers? _linkType;
        private Uri _uri;

        public PcapLoader(ILoggerFactory loggerFactory) => this._logger = loggerFactory.CreateLogger<PcapLoader>();

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            this._logger.LogInformation($"Closing> {this._uri}");
            this.Dispose();
        }

        public RawCapture GetNextPacket() => this._captureDevice.GetNextPacket();

        public LinkLayers LinkType => (this._linkType ?? (this._linkType = this._captureDevice?.LinkType ?? LinkLayers.Null)).Value;

        public void Open(Uri uri)
        {
            if (this._captureDevice != null)
            {
                throw new InvalidOperationException("Only one call per object.");
            }
            this._uri = uri;

            this._captureDevice = new CaptureFileReaderDevice(this._uri.AbsolutePath);

            this._logger.LogInformation($"Opening> {this._uri}");
            this._captureDevice.Open();
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(Boolean disposing)
        {
            if (this._disposed)
            {
                return;
            }


            if (disposing)
            {
            }

            //TODO, does this work? Or it will access disposed logger?
            this._logger.LogInformation($"Closing> {this._uri}");
            this._captureDevice?.Close();
            this._disposed = true;
        }
    }
}
