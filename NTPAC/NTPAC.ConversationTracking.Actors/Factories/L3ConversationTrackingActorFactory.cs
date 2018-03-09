using System;
using System.Collections.Generic;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L3ConversationTrackingActorFactory : IL3ConversationTrackingActorFactory
  {
    private readonly IServiceProvider _serviceProvider;
    public L3ConversationTrackingActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context,
                            IL3ConversationKey l3Key,
                            IActorRef contractor,
                            List<IActorRef> l7ConversationHandlerActors) =>
      context.ActorOf(L3ConversationTrackingActor.Props(l3Key, contractor, l7ConversationHandlerActors,
                                                        this._serviceProvider
                                                            .GetRequiredService<IL4ConversationTrackingActorFactory>())
#if DEBUG
                     , l3Key.ToString()
#endif
        );
  }
}
