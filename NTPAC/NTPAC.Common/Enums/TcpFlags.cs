using System;

namespace NTPAC.Common.Enums
{
    [Flags]
    public enum TcpFlags
    {
        None = 0,
        Fin = 1 << 0,
        Syn = 1 << 1,
        Rst = 1 << 2,
        Psh = 1 << 3,
        Ack = 1 << 4,
        Urg = 1 << 5,
        Ecn = 1 << 6,
        Cwr = 1 << 7
    }
}
