using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Core.Snoopers
{
  public abstract class SnooperBase
  {
    protected IL7Conversation CurrentL7Conversation;
    protected IPduReader PduReader;

    public SnooperExportCollection ProcessConversation(IL7Conversation conversation, Boolean stopProcessingAfterError = true)
    {
      this.CurrentL7Conversation = conversation;
      this.PduReader = this.CreatePduReader();

      var snooperExports = new List<SnooperExportBase>();
      using (var snooperExportsEnumerator = this.ProcessConversation().GetEnumerator())
      {
        try
        {
          while (snooperExportsEnumerator.MoveNext())
          {
            var snooperExport = snooperExportsEnumerator.Current;
            snooperExports.Add(snooperExport);
            
            if (stopProcessingAfterError && snooperExport.ParsingFailed)
            {
              break;
            }
          }
        }
        catch (Exception e)
        {
          var unhandledExceptionExport = new SnooperUnhandledExceptionExport(this.PduReader, e);
          snooperExports.Add(unhandledExceptionExport); 
        }     
      }
      var snooperExportCollection = new SnooperExportCollection(this, snooperExports);
      return snooperExportCollection;
    }

    protected abstract IPduReader CreatePduReader();
    protected abstract IEnumerable<SnooperExportBase> ProcessConversation();
    public abstract String SnooperId { get; }
    // Protocol names from service-names-port-numbers.csv
    public abstract String[] ApplicationProtocolTags { get; }
  }
}
