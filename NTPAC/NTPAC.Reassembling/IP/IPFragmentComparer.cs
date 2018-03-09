using System;
using System.Collections.Generic;
using NTPAC.ConversationTracking.Models;

namespace NTPAC.Reassembling.IP
{
  public class IPFragmentComparer : IComparer<Frame>
  {
    public Int32 Compare(Frame x, Frame y) => x.Ipv4FragmentOffset - y.Ipv4FragmentOffset;
  }
}
