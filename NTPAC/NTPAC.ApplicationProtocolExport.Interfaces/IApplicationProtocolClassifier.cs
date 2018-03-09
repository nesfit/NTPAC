using System;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Interfaces
{
  public interface IApplicationProtocolClassifier
  {
    String Classify(IL7Conversation l7Conversation);
  }
}
