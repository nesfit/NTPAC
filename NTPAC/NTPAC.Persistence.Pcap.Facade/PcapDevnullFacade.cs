using System.Threading.Tasks;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.Persistence.Pcap.Facade
{
  public class PcapDevnullFacade : IPcapFacade
  {
    public void StoreL7Conversation(L7Conversation l7Conversation) {  }

    public Task StoreL7ConversationAsync(L7Conversation l7Conversation) => Task.CompletedTask;
  }
}
