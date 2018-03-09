using System;
using System.Linq;
using Akka.DistributedData;
using Newtonsoft.Json;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperHTTP.Models
{
  public enum HttpMessageType
  {
    None = 0,
    Request = 1,
    Response = 2
  }
  
  public class HttpMessage : SnooperExportBase
  {
    public HttpMessageType Type { get; private set; }
    
    public HttpHeaderBase Header { get; private set; }
    
    [JsonIgnore]   
    public HttpContent Content { get; private set; }
   
    public HttpMessage(PduStreamReader reader) : base(reader) 
    {      
      try
      {
        this.Parse(reader);
      }
      catch (HttpParsingException e)
      {
        this.ParsingError = e;
      }
    }

    private void Parse(PduStreamReader reader)
    {
      if (!this.ParseHeader(reader))
      {
        return;
      }

      if (this.IsProbeRequest())
      {
        return;
      }
      this.ParseContent(reader);
    }
      
    private Boolean ParseHeader(PduStreamReader reader)
    {
      var headerLine = reader.ReadLine();
      if (headerLine == null)
      {
        this.ParsingError = new HttpParsingException("Nothing to read");
        return false;
      }

      if(Enum.TryParse(headerLine.Split(' ').FirstOrDefault(), true, out HttpRequestMethod httpRequestMethod))
      {
        // HTTP Request
        this.Header = new HttpHeaderRequest(reader, headerLine, httpRequestMethod);
      }
      else if (headerLine.StartsWith("HTTP"))
      {
        // HTTP Response
        this.Header = new HttpHeaderResponse(reader, headerLine);
      }
      else
      {
        this.ParsingError = new HttpParsingException("Not a HTTP message");
        return false;
      }
      
      switch (this.Header)
      {
        case HttpHeaderRequest _:
          this.Type = HttpMessageType.Request;
          break;
        case HttpHeaderResponse _:
          this.Type = HttpMessageType.Response;
          break;
        default:
          this.Type = HttpMessageType.None;
          break;
      }

      return true;
    }
 
    private Boolean ParseContent(PduStreamReader reader)
    {
      TransferEncoding transferEncoding;
      if (this.Header.HttpVersion.Equals("HTTP/1.0"))
      {
        transferEncoding = TransferEncoding.Http1Transfer;
      }
      else
      {
        var transferEncodingStr = this.Header.GetLastHeaderFieldValue("Transfer-Encoding", "identity");
        if (!Enum.TryParse(transferEncodingStr, true, out transferEncoding))
        {
          this.ParsingError = new HttpParsingException($"Unknown TransferEncoding: {transferEncodingStr}");
          return false;
        }
      }
      
      var contentEncodingStr = this.Header.GetLastHeaderFieldValue("Content-Encoding", "identity");
      if (!Enum.TryParse(contentEncodingStr, true, out ContentEncoding contentEncoding))
      {
        this.ParsingError = new HttpParsingException($"Unknown ContentEncoding: {contentEncodingStr}");
        return false;
      }

      Int32? contentLength = null;
      var contentLengthStr = this.Header.GetLastHeaderFieldValue("Content-Length");
      if (contentLengthStr != null)
      {
        if (!Int32.TryParse(contentLengthStr, out var contentLengthVal))
        {
          this.ParsingError = new HttpParsingException($"Failed to parse ContentLength: {contentLengthStr}");
          return false;
        }
        contentLength = contentLengthVal;
      }
       
      this.Content = new HttpContent(reader, transferEncoding, contentLength, contentEncoding);
      
      return true;
    }

    public void AddContentToProbingRequest(PduStreamReader reader)
    {
      this.ParseContent(reader);
    }
      
    public Boolean IsProbeRequest() => this.Type == HttpMessageType.Request
                                         && this.Header.HeaderFieldValueEquals("Expect", "100-continue");

    public Boolean IsProbeResponseSuccess()
    {
      var httpResponseHeader = (HttpHeaderResponse) this.Header;
      return this.Type == HttpMessageType.Response               &&
             httpResponseHeader.StatusMessage.Equals("Continue") &&
             httpResponseHeader.StatusCode == 100;
    }
    
    public Boolean ShouldIgnorePayload => this.Content?.Payload?.Length >= 25_000_000; // 25 MB
  }
}
