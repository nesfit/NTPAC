using System;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.Common.Interfaces
{
    public interface IPcapLoader : IDisposable
    {
        LinkLayers LinkType { get; }
        void Close();
        RawCapture GetNextPacket();
        void Open(Uri uri);
    }
}
