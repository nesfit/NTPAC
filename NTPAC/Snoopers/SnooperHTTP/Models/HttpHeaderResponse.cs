using System;
using System.Linq;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperHTTP.Models
{
  public class HttpHeaderResponse : HttpHeaderBase
  {
    public readonly String Version;
    public readonly Int16 StatusCode;
    public readonly String StatusMessage;
    
    public HttpHeaderResponse(PduStreamReader reader, String headerLine)
    {
      var headerLineComponents = headerLine.Split(' ');
      this.Version = headerLineComponents[0];
      this.StatusCode = Convert.ToInt16(headerLineComponents[1]);
      this.StatusMessage = String.Join(" ", headerLineComponents.Skip(2));
      
      this.ParseHeaderValues(reader);
    }

    public override String StatusLine => this.Version + " " + this.StatusCode + " " + this.StatusMessage;
    
    public override String HttpVersion => this.Version;
  }
}
