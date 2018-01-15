using System;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.Common.Models
{
    public class RawCaptureN : RawCapture
    {
        public RawCaptureN(LinkLayers LinkLayerType, PosixTimeval Timeval, Byte[] Data) : base(LinkLayerType, Timeval, Data)
        {
        }
        
        public RawCaptureN(LinkLayers LinkLayerType, PosixTimeval Timeval, Byte[] Data, Int64 num) : this(LinkLayerType, Timeval, Data)
            => this.Num = num;

        public readonly Int64 Num;
    }
}
