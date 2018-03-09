using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Actors.Factories;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors
{
  public static class ActorsServiceInstaller
  {
    public static void Install(IServiceCollection services)
    {
      services.AddSingleton<IRawPacketBatchParserActorFactory, RawPacketBatchParserActorFactory>();
      services.AddSingleton<ICaptureTrackingActorFactory, CaptureTrackingActorFactory>();
      services.AddSingleton<IL3ConversationTrackingActorFactory, L3ConversationTrackingActorFactory>();
      services.AddSingleton<IL4ConversationTrackingActorFactory, L4ConversationTrackingActorFactory>();
      services.AddSingleton<IL7ConversationStorageActorFactory, L7ConversationStorageActorFactory>();
      services.AddSingleton<IApplicationProtocolExportActorFactory, ApplicationProtocolExportActorFactory>();
    }
  }
}
