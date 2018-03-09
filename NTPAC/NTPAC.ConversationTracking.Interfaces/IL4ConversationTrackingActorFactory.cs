using System;
using System.Collections.Generic;
using System.Net;
using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL4ConversationTrackingActorFactory
  {
    IActorRef Create(IActorContext context,
                     IL4ConversationKey l4Key,
                     IActorRef contractor,
                     IPEndPoint sourceEndPoint,
                     IPEndPoint destinationEndPoint,
                     List<IActorRef> l7ConversationHandlersActors,
                     Int64 timestampTicks
                     );
  }
}
