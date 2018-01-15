using System;
using System.Threading.Tasks;

namespace NTPAC.Common.Interfaces
{
    public interface IPacketIngestor
    {
        void OpenPcap(Uri uri);
        Task OpenPcapAsync(Uri uri);
    }
}
