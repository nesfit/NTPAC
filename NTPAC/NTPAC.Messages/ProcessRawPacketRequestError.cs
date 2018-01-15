using System;

namespace NTPAC.Messages
{
    public class ProcessRawPacketRequestError : ProcessRawPacketRequest
    {
        public ProcessRawPacketRequestError(String description) => this.Description = description;

        public String Description { get; }
    }
}
