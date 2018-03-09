using System;
using System.Collections.Generic;
using System.Net;
using Akka.Actor;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L4ConversationTrackingActorFactory : IL4ConversationTrackingActorFactory
  {
    public IActorRef Create(IActorContext context,
                            IL4ConversationKey l4Key,
                            IActorRef contractor,
                            IPEndPoint sourceEndPoint,
                            IPEndPoint destinationEndPoint,
                            List<IActorRef> l7ConversationHandlersActors,
                            Int64 timestampTicks) =>
      context.ActorOf(L4ConversationTrackingActor.Props(l4Key, contractor, sourceEndPoint, destinationEndPoint,
                                                        l7ConversationHandlersActors)
#if DEBUG
                      , $"{l4Key.ToString()}_{timestampTicks}"
#endif
        );
  }
}
