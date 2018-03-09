using System;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperHTTP.Models
{
  public enum HttpRequestMethod
  {
    Options,
    Get,
    Head,
    Post,
    Put,
    Delete,
    Trace,
    Connect,
    PropFind
  }

  public class HttpHeaderRequest : HttpHeaderBase
  {
    public readonly HttpRequestMethod Method;
    public readonly String RequestUri;
    public readonly String Version;
    
    public HttpHeaderRequest(PduStreamReader reader, String headerLine, HttpRequestMethod method)
    {
      this.Method = method;

      var headerLineComponents = headerLine.Split(' ');
      this.RequestUri = headerLineComponents[1];
      this.Version = headerLineComponents[2];
      
      this.ParseHeaderValues(reader);
    }

    public override String StatusLine => this.Method + " " + this.RequestUri + " " + this.Version;

    public override String HttpVersion => this.Version;
  }
}
