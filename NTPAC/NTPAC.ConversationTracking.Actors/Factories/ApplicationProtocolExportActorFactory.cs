using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class ApplicationProtocolExportActorFactory : IApplicationProtocolExportActorFactory
  {
    private readonly IServiceProvider _serviceProvider;
    
    public ApplicationProtocolExportActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, IActorRef contractor, Predicate<IL7Conversation> l7ConversationFilter) =>
      context.ActorOf(ApplicationProtocolExportActor.Props(contractor, l7ConversationFilter, this._serviceProvider.GetRequiredService<ISnooperExportFacade>()),
                      "APE");
  }
}
