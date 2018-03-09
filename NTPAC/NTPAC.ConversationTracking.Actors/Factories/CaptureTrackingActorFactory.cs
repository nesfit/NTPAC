using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class CaptureTrackingActorFactory : ICaptureTrackingActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public CaptureTrackingActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, ICaptureInfo captureInfo, IActorRef contractor) =>
      context.ActorOf(CaptureTrackingActor.Props(captureInfo, contractor,
                                                 this._serviceProvider.GetRequiredService<IL3ConversationTrackingActorFactory>(),
                                                 this._serviceProvider.GetRequiredService<IL7ConversationStorageActorFactory>(),
                                                 this._serviceProvider.GetRequiredService<IApplicationProtocolExportActorFactory>()
                        ));
  }
}
