using System.Collections.Generic;
using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL3ConversationTrackingActorFactory
  {
    IActorRef Create(IActorContext context, IL3ConversationKey l3Key, IActorRef contractor, List<IActorRef> l7ConversationHandlerActors);
  }
}
