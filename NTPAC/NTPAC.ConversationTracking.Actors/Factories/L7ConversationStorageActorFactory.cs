using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L7ConversationStorageActorFactory : IL7ConversationStorageActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public L7ConversationStorageActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, IActorRef contractor, ICaptureInfo captureInfo) =>
      context.ActorOf(L7ConversationStorageActor.Props(contractor, captureInfo,
                                                       this._serviceProvider.GetRequiredService<ICaptureFacade>(),
                                                       this._serviceProvider.GetRequiredService<IL7ConversationFacade>(),
                                                       this._serviceProvider.GetRequiredService<IPcapFacade>()),
                      "L7CS");
  }
}
