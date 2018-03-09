using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NTPAC.Common
{
  public class RelativeFileUri : Uri
  {  
    public RelativeFileUri(String uriString) : base(TransformUriString(uriString))
    {
    }

    private static String TransformUriString(String uriString)
    {
      // Do nothing if an URI scheme (file://, smb://, http://, ...) is specified
      const String uriSchemePattern = @"^[\w]+:\/\/";
      if (Regex.IsMatch(uriString, uriSchemePattern, RegexOptions.IgnoreCase))
      {
        return uriString;
      }
      
      // Otherwise assume it's a file
      return Path.GetFullPath(Environment.ExpandEnvironmentVariables(uriString));
    }
  }
}
