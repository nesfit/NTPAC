using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.SnooperExportEntities;
using NTPAC.Persistence.Generic.Facades.Installers;
using UnitOfWork;
using UnitOfWork.InMemoryRepository;
using UnitOfWork.InMemoryUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.InMemory.Facades
{
  public static class InMemoryServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection)
    {
      EntityRepositoriesServiceInstaller.InstallUnitOfWork(serviceCollection, typeof(InMemoryUnitOfWork));
      EntityRepositoriesServiceInstaller.InstallEntityRepositories(serviceCollection, typeof(BaseRepository<>));  
    }
  }
}
