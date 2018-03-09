using System;

namespace NTPAC.Messages.L4ConversationTracking
{
  public class L4ConversationTrackingComplete
  {
    public static readonly L4ConversationTrackingComplete Instance = new L4ConversationTrackingComplete();

    public static readonly L4ConversationTrackingComplete InactivityInstance = new L4ConversationTrackingComplete
                                                                               {
                                                                                 CompletedByInactivity = true
                                                                               };

    public Boolean CompletedByInactivity { private set; get; }

    private L4ConversationTrackingComplete() { }
  }
}
