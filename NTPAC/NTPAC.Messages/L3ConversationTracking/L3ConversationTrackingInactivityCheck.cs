using System;

namespace NTPAC.Messages.L3ConversationTracking
{
  public class L3ConversationTrackingInactivityCheck
  {
    public readonly Int64 CurrentTimestampTicks;
    public readonly Int64 InactivityTimeoutTicks;

    public L3ConversationTrackingInactivityCheck(Int64 currentTimestampTicks, Int64 inactivityTimeoutTicks)
    {
      this.CurrentTimestampTicks = currentTimestampTicks;
      this.InactivityTimeoutTicks = inactivityTimeoutTicks;
    }
  }
}
