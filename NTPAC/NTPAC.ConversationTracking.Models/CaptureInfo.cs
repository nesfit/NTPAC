using System;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ConversationTracking.Models
{
  public class CaptureInfo : ICaptureInfo
  {
    public CaptureInfo() { }
    public CaptureInfo(Uri uri) => this.UriString = uri.AbsoluteUri;
    public Uri Uri => new Uri(this.UriString);
    public String UriString { get; set; }
  }
}
