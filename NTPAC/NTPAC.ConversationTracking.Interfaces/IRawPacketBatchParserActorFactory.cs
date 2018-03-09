using Akka.Actor;

namespace NTPAC.ConversationTracking.Interfaces
{
  public interface IRawPacketBatchParserActorFactory
  {
    IActorRef Create(IActorContext context, IActorRef contractor, ICaptureInfo captureInfo);
  }
}
