using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class RawPacketBatchParserActorFactory : IRawPacketBatchParserActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public RawPacketBatchParserActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, IActorRef contractor, ICaptureInfo captureInfo) =>
      context.ActorOf(RawPacketBatchParserActor.Props(this._serviceProvider.GetRequiredService<ICaptureTrackingActorFactory>(),
                                                      contractor, captureInfo));
  }
}
