using System.Threading.Tasks;
using NTPAC.ConversationTracking.Models;

namespace NTPAC.Persistence.Interfaces
{
  public interface IPcapFacade
  {
    void StoreL7Conversation(L7Conversation l7Conversation);
    Task StoreL7ConversationAsync(L7Conversation l7Conversation);
  }
}
