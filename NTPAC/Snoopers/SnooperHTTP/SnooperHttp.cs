using System;
using System.Collections.Generic;
using System.Diagnostics;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;
using NTPAC.ApplicationProtocolExport.Interfaces;
using SnooperHTTP.Models;

namespace SnooperHTTP
{
  public class SnooperHttp : SnooperBase
  {
    public override String SnooperId => "HTTP";
    
    public override String[] ApplicationProtocolTags => new [] {"www-http"};

    protected override IPduReader CreatePduReader()
    {
      var pduDataStream   = new PduDataStream(this.CurrentL7Conversation, PduDataProviderType.Breaked);
      var pduStreamReader = new PduStreamReader(pduDataStream);
      return pduStreamReader;
    }

    protected override IEnumerable<SnooperExportBase> ProcessConversation()
    {
      var pduStreamReader = (PduStreamReader) this.PduReader;
      if (pduStreamReader.EndOfStream)
      {
        yield break;
      }

      // To handle https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Expect
      HttpMessage probeRequestMessage = null;
      var state = SnooperHttpState.ExpectingMessage;
      do
      {
        HttpMessage httpMessage;
        
        switch (state)
        {
          case SnooperHttpState.ExpectingMessage:
            httpMessage = new HttpMessage(pduStreamReader);
            if (httpMessage.IsProbeRequest())
            {
              probeRequestMessage = httpMessage;
              state = SnooperHttpState.ExpectingProbeResponse;
              continue;
            }
            yield return httpMessage;
            break;
          case SnooperHttpState.ExpectingProbeResponse:
            httpMessage = new HttpMessage(pduStreamReader);
            state = httpMessage.IsProbeResponseSuccess() ? SnooperHttpState.ExpectingProbeRequestContent : SnooperHttpState.ExpectingMessage;
            break;
          case SnooperHttpState.ExpectingProbeRequestContent:
            // ReSharper disable once PossibleNullReferenceException
            probeRequestMessage.AddContentToProbingRequest(pduStreamReader);
            state = SnooperHttpState.ExpectingMessage;
            yield return probeRequestMessage;
            probeRequestMessage = null;
            break;
        }
      } while (pduStreamReader.NewMessage()); 
    }

    private enum SnooperHttpState
    {
      ExpectingMessage,
      ExpectingProbeResponse,
      ExpectingProbeRequestContent,
    }
   
  }
}
