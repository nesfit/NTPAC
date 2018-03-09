using System;
using System.Collections.Generic;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;
using NTPAC.ApplicationProtocolExport.Interfaces;
using SnooperTLS.Models;

namespace SnooperTLS
{
  public class SnooperTls : SnooperBase
  {
    public override String SnooperId => "TLS";
    public override String[] ApplicationProtocolTags => new[] { "https" };

    protected override IPduReader CreatePduReader()
    {
      var pduDataStream   = new PduDataStream(this.CurrentL7Conversation, PduDataProviderType.Mixed);
      var pduKaitaiReader = new PduKaitaiReader(pduDataStream);
      return pduKaitaiReader;
    }

    protected override IEnumerable<SnooperExportBase> ProcessConversation()
    {
      var pduKaitaiReader = (PduKaitaiReader) this.PduReader;
      if (pduKaitaiReader.EndOfStream)
      {
        yield break;
      }
      
      do
      {
        var tlsMessage = new TlsMessage(pduKaitaiReader);
        yield return tlsMessage;
      } while (pduKaitaiReader.NewMessage());
    }
  }
}
