using System.Collections.Generic;
using Akka.Remote;

namespace NTPAC.Messages
{
    public class ProcessRawPacketFragmentsRequest : ProcessRawPacketRequest
    {
        public ProcessRawPacketFragmentsRequest(IEnumerable<ProcessRawPacketRequest> fragmentRequests) => this.FragmentRequests = fragmentRequests;
        
        public readonly IEnumerable<ProcessRawPacketRequest> FragmentRequests;
        
        
        public static readonly ProcessRawPacketFragmentsRequest EmptyInstance = new ProcessRawPacketFragmentsRequest(new ProcessRawPacketRequest[0]);
    }
}
