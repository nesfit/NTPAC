using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperHTTP.Models
{
  public enum TransferEncoding
  {
    Identity,
    Chunked,
    Http1Transfer
  }

  public enum ContentEncoding
  {
    Identity,
    Gzip,
    Deflate
  }
  
  public class HttpContent
  {
    public Byte[] Payload { get; private set; }
    
    public Boolean PayloadIncomplete { get; private set; }
    
    public HttpContent(PduStreamReader reader, TransferEncoding transferEncoding, Int32? contentLength, ContentEncoding contentEncoding)
    {
      this.ParseTransfer(reader, transferEncoding, contentLength);
      this.DecodeContent(contentEncoding);
    }

    private void ParseTransfer(PduStreamReader reader, TransferEncoding transferEncoding, Int32? contentLength)
    {
      switch (transferEncoding)
      {
        case TransferEncoding.Identity:
          this.ParseTransferIdentity(reader, contentLength ?? 0);
          break;
        case TransferEncoding.Chunked:
          this.ParseTransferChunked(reader);
          break;
        case TransferEncoding.Http1Transfer:
          this.ParseTransferHttp1(reader);
          break;
        default:
          throw new HttpParsingException($"Unsupported TransferEncoding: {transferEncoding}");          
      }
    }
    
    private void ParseTransferIdentity(PduStreamReader reader, Int32 contentLength)
    {
      this.Payload = new Byte[contentLength];
      if (contentLength == 0)
      {
        return;
      }

      var readTotal = 0;
      Int32 read;
      do
      {
        read = reader.Read(this.Payload, readTotal, contentLength - readTotal);
        readTotal += read;
      } while (readTotal < contentLength && read != 0);

      if (contentLength != readTotal)
      {
        this.PayloadIncomplete = true;
      }
    }

    private void ParseTransferChunked(PduStreamReader reader)
    {
      var chunks = new List<Byte[]>();
      
      while (true)
      {
        var chunk = ReadChunk(reader);
        if (chunk == null)
        {
          break;
        }
        chunks.Add(chunk);
      }

      var totalContentSize = chunks.Select(chunk => chunk.Length).Sum();
      this.Payload = new Byte[totalContentSize];
      var writtenBytes = 0;
      foreach (var chunk in chunks)
      {
        chunk.CopyTo(this.Payload, writtenBytes);
        writtenBytes += chunk.Length;
      }
    }

    private void ParseTransferHttp1(PduStreamReader reader)
    {
      var payloadBuffer = new List<Byte>();
      
      var buffer = new Byte[4096];
      while ((reader.Read(buffer, 0, buffer.Length)) > 0)
      {
        payloadBuffer.AddRange(buffer);
      }

      this.Payload = payloadBuffer.ToArray();
    }
    
    private Byte[] ReadChunk(PduStreamReader reader)
    {
      var chunkSizeLine = reader.ReadLine()?.Trim();
      if (chunkSizeLine == null)
      {
        return null;
      }
      var chunkSize = Convert.ToInt32(chunkSizeLine, 16);
      if (chunkSize == 0)
      {
        return null;
      }

      var chunkBuffer = new Byte[chunkSize];
      var readTotal = 0;
      do
      {
        var read = reader.Read(chunkBuffer, readTotal, chunkSize - readTotal);
        if(read == 0)
        {
          return null;
        }
        readTotal += read;
      } while(readTotal < chunkSize);

      if (reader.ReadLine() == null)
      {
        return null;
      }

      return chunkBuffer;
    }

    private void DecodeContent(ContentEncoding contentEncoding)
    {
      if (contentEncoding == ContentEncoding.Identity || this.Payload.Length == 0 || this.PayloadIncomplete)
      {
        return;
      }
      
      var inputStream  = new MemoryStream(this.Payload);
      var outputStream = new MemoryStream();
      var buffer       = new Byte[4096];

      Stream decoderStream; 
      switch(contentEncoding)
      {
        case ContentEncoding.Gzip:
          decoderStream = new GZipStream(inputStream, CompressionMode.Decompress);
          break;
        case ContentEncoding.Deflate:
          decoderStream = new DeflateStream(inputStream, CompressionMode.Decompress);
          break;
        default:
          throw new HttpParsingException($"Unsupported ContentEncoding: {contentEncoding}");
      }

      Int32 decodedBytes;
      while ((decodedBytes = decoderStream.Read(buffer, 0, buffer.Length)) != 0)
      {
        outputStream.Write(buffer, 0, decodedBytes);
      }
      
      this.Payload = outputStream.ToArray();
    }
  }
}
