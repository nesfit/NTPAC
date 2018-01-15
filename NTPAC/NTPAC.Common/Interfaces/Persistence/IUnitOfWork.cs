using System;

namespace NTPAC.Common.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Int32 Complete();
    }
}