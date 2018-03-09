using System;
using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IApplicationProtocolExportActorFactory
  {
    IActorRef Create(IActorContext context, IActorRef contractor, Predicate<IL7Conversation> l7ConversationFilter);
  }
}
