using System;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL3ConversationKey
  {
    Byte[] Key { get; set; }
    Boolean Equals(Object obj);
    Int32 GetHashCode();
    String ToString();
  }
}
