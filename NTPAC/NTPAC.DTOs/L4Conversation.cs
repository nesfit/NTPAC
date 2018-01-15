using System.Net;

namespace NTPAC.DTOs
{
    public class L4Conversation
    {
        public IPEndPoint SourceEndPoint { get; set; }
        public IPEndPoint DestinationEndPoint { get; set; }
    }
}