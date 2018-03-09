using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.Persistence.Generic.Facades.Installers
{
  public static class FacadesServiceInstaller
  {
    public static void Install(IServiceCollection services)
    {
      services.AddSingleton<ICaptureFacade, CaptureFacade>();
      services.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();
      services.AddSingleton<ISnooperExportFacade, SnooperExportFacade>();
    }
  }
}
