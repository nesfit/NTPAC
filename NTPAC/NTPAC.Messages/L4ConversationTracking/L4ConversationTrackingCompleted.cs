using System;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.Messages.L4ConversationTracking
{
  public class L4ConversationTrackingCompleted
  {
    public L4ConversationTrackingCompleted(IL4ConversationKey l4ConversationKey, Boolean completedByInactivity = false)
    {
      this.L4ConversationKey = l4ConversationKey;
      this.CompletedByInactivity = completedByInactivity;
    }
    public IL4ConversationKey L4ConversationKey { get; }
    public Boolean CompletedByInactivity { get; }
  }
}
