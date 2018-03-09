using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ApplicationProtocolExport.Core.Exceptions;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using SnooperTLS.Kaitai;

namespace SnooperTLS.Models
{
  public class TlsMessage : SnooperExportBase
  {
    public IReadOnlyCollection<TlsRecordBase> Records { get; private set; }
    
    public TlsMessage(PduKaitaiReader reader) : base(reader)
    {
      try
      {
        this.Parse(reader);
      }
      catch (KaitaiObjectionConstructionException e)
      {
        this.ParsingError = e;
      }
      catch (TlsParsingException e)
      {
        this.ParsingError = e;
      }
    }

    private void Parse(PduKaitaiReader reader)
    {
      var tlsPacket = reader.ReadKaitaiStruct<TlsPacket>();
      this.Records = tlsPacket.Records.Select(this.ParseKaitaiTlsRecord).ToArray();
    }

    private TlsRecordBase ParseKaitaiTlsRecord(TlsPacket.TlsRecord kaitaiTlsRecord)
    {
      TlsRecordBase tlsRecord;
      
      switch (kaitaiTlsRecord.Fragment)
      {
        case TlsPacket.TlsHandshake f:
          tlsRecord = new TlsRecordHandshake(f);
          break;
        case TlsPacket.TlsChangeCipherSpec f:
          tlsRecord = new TlsRecordChangeCipherSpec(f);
          break;
        case TlsPacket.TlsEncryptedMessage f:
          tlsRecord = new TlsRecordAlert(f);
          break;
        case TlsPacket.TlsApplicationData f:
          tlsRecord = new TlsRecordApplicationData(f);
          break;
        default:
          throw new TlsParsingException($"Unknown TLS record content type: {kaitaiTlsRecord.ContentType}");
      }
      
      return tlsRecord;
    }
  }
}
