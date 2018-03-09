using BaseDataEntity;
using Repository;
using UnitOfWork;

namespace NTPAC.Common.Interfaces
{
  public interface IRepositoryFactory
  {
    IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, IDataEntity, new();
    IUnitOfWork CreateUnitOfWork();
  }
}
