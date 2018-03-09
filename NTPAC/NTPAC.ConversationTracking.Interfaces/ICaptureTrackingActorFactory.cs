using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface ICaptureTrackingActorFactory
  {
    IActorRef Create(IActorContext context, ICaptureInfo captureInfo, IActorRef contractor);
  }
}
