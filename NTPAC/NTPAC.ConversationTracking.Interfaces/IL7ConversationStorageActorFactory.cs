using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IL7ConversationStorageActorFactory
  {
    IActorRef Create(IActorContext context, IActorRef contractor, ICaptureInfo captureInfo);
  }
}
