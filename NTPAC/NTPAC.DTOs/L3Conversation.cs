using System;
using System.Net;

namespace NTPAC.DTOs
{
    public class L3Conversation
    {
        public IPEndPoint SourceEndPoint { get; set; } 
        public IPEndPoint DestinationEndPoint { get; set; } 
    }
}
