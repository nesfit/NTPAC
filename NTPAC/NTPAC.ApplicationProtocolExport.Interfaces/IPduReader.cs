using System;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Interfaces
{
  public interface IPduReader
  {
    IL7Pdu CurrentPdu { get; }
    IL7Conversation CurrentConversation { get; }
    Boolean EndOfStream { get; }
    Boolean NewMessage();
  }
}
