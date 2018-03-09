using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Generic.Facades.Installers;
using UnitOfWork.DevnullRepository;
using UnitOfWork.DevnullUnitOfWork;

namespace NTPAC.Persistence.DevNull.Facades
{
  public static class DevNullServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection)
    {
      EntityRepositoriesServiceInstaller.InstallUnitOfWork(serviceCollection, typeof(DevnullUnitOfWork));
      EntityRepositoriesServiceInstaller.InstallEntityRepositories(serviceCollection, typeof(BaseRepository<>));  
    }
  }
}
