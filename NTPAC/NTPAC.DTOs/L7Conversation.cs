using System.Net;

namespace NTPAC.DTOs
{
    public class L7Conversation
    {
        public IPEndPoint SourceEndPoint { get; set; }
        public IPEndPoint DestinationEndPoint { get; set; }
    }
}