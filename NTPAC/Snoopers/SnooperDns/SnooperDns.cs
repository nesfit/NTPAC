using System;
using System.Collections.Generic;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;
using NTPAC.ApplicationProtocolExport.Interfaces;
using SnooperDNS.Models;

namespace SnooperDNS
{
  public class SnooperDns : SnooperBase
  {
    public override String SnooperId => "DNS";
    public override String[] ApplicationProtocolTags => new[] {"domain"};

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
        var dnsMessage = new DnsMessage(pduKaitaiReader);
        yield return dnsMessage;
      } while (pduKaitaiReader.NewMessage());
    }
  }
}
