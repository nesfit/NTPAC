using NTPAC.ConversationTracking.Models;

namespace NTPAC.Persistence.Entities.Mappers
{
  public static class CaptureMapper
  {
    public static CaptureEntity Map(Capture capture) =>
      new CaptureEntity
      {
        Uri                 = capture.Info.UriString,
        Processed           = capture.Processed,
        Id                  = capture.Id,
        L7ConversationCount = capture.L7ConversationCount,
        FirstSeen           = capture.FirstSeen,
        LastSeen            = capture.LastSeen
      };
  }
}
