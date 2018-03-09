using System;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;

namespace NTPAC.ApplicationProtocolExport.Core.Models
{
  public abstract class SnooperExportBase
  {
    public readonly Guid ConversationId;
    public readonly DateTime Timestamp;
    public readonly FlowDirection Direction;
    
#if DEBUG
    public readonly IL7Conversation Conversation;
    public readonly IL7Pdu Pdu;
#else
    public readonly String Conversation;
    public readonly String Pdu;
#endif
    
    public Exception ParsingError { get; protected set; }
    public Boolean ParsingFailed => this.ParsingError != null;
  
    protected SnooperExportBase(IPduReader reader)
    {
      this.ConversationId = reader.CurrentConversation.Id;
      this.Timestamp = reader.CurrentPdu.FirstSeen;
      this.Direction = reader.CurrentPdu.Direction;
      
#if DEBUG
      this.Conversation = reader.CurrentConversation;
      this.Pdu          = reader.CurrentPdu;
#else
      this.Conversation = reader.CurrentConversation.ToString();
      this.Pdu          = reader.CurrentPdu.ToString();
#endif
    }
  }
}
