using System;

namespace NTPAC.Messages
{
    public class StartProcessingRequest
    {
        public StartProcessingRequest(String pcapFilePath) => this.PcapFilePath = pcapFilePath;

        public String PcapFilePath { get; }
    }
}
