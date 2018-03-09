using System.Collections.Generic;
using System.Threading.Tasks;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Interfaces
{
  public interface IL7ConversationProvider
  {
    Task<IEnumerable<IL7Conversation>> LoadAsync();
  }
}
