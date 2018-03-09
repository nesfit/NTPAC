using System;
using System.Collections.Generic;
using NTPAC.Common.Helpers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;

namespace NTPAC.Reassembling.Tests.Comparers
{
  public class L7PDUComparer : IEqualityComparer<L7Pdu>, IEqualityComparer<IL7Pdu>
  {
    public Boolean Equals(L7Pdu x, L7Pdu y) => MemberCompare.Equal(x, y);

    public Int32 GetHashCode(L7Pdu obj) => obj.GetHashCode();

    public Boolean Equals(IL7Pdu x, IL7Pdu y) => MemberCompare.Equal(x, y);

    public Int32 GetHashCode(IL7Pdu obj) => obj.GetHashCode();
  }
}
