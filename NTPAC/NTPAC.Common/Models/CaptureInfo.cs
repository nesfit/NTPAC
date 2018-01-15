using System;

namespace NTPAC.Common.Models
{
    public class CaptureInfo
    {
        public CaptureInfo(String filename) => this.Filename = filename;

        public String Filename { get; }
    }
}
