using System;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL4ConversationKey
  {
    IPProtocolType GetProtocolType { get; }
    Boolean Equals(Object obj);
    Int32 GetHashCode();
    String ToString();
  }
}
