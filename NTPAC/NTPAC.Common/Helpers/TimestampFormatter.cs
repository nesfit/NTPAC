using System;

namespace NTPAC.Common.Helpers
{
  public static class TimestampFormatter
  {
    public static String Format(Int64 ticks) => Format(new DateTime(ticks));
    
    public static String Format(DateTime dateTime) => dateTime.ToLocalTime().ToString("HH:mm:ss.ffffff");
  }
}
