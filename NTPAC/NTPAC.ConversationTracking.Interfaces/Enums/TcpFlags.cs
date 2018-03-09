using System;

namespace NTPAC.ConversationTracking.Interfaces.Enums
{
  [Flags]
  public enum TcpFlags
  {
    None = 0,
    Ack = 1 << 0,
    Cwr = 1 << 1,
    Ecn = 1 << 2,
    Fin = 1 << 3,
    Psh = 1 << 4,
    Rst = 1 << 5,
    Syn = 1 << 6,
    Urg = 1 << 7,
  }
}
