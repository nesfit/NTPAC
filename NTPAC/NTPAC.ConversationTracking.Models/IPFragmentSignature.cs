using System;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Models
{
  public class IPFragmentSignature
  {
    public IPFragmentKey IPFragmentKey;
    public readonly Boolean MoreFragments;
    public readonly Int32 FragmentOffset;

    public IPFragmentSignature(IPAddress sourceAddress, IPAddress destinationAddress, IPProtocolType ipProtocolType, UInt16 identification, Boolean moreFragments, Int32 fragmentOffset)
    {
      this.IPFragmentKey = new IPFragmentKey(sourceAddress, destinationAddress, ipProtocolType, identification);
      this.MoreFragments      = moreFragments;
      this.FragmentOffset     = fragmentOffset;
    }
  }
}
