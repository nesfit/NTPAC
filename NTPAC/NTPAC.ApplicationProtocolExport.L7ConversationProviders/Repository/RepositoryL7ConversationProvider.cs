using System.Collections.Generic;
using System.Threading.Tasks;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ApplicationProtocolExport.L7ConversationProviders.Repository
{
  public class RepositoryL7ConversationProvider : IL7ConversationProvider
  {
    private readonly ICaptureFacade _captureFacade;
    private readonly IL7ConversationFacade _l7ConversationFacade;
    private readonly RepositoryL7ConversationProviderOptions _opts;
    
    public RepositoryL7ConversationProvider(ICaptureFacade captureFacade, IL7ConversationFacade l7ConversationFacade, RepositoryL7ConversationProviderOptions opts)
    {
      this._captureFacade = captureFacade;
      this._l7ConversationFacade = l7ConversationFacade;
      this._opts = opts;
    }

    public async Task<IEnumerable<IL7Conversation>> LoadAsync()
    {
      IEnumerable<IL7Conversation> l7Conversations;
      
      if (this._opts.L7ConversationFilterPredicate != null)
      {
        l7Conversations = await this._l7ConversationFacade.GetAllWhereForProcessingAsync(this._opts.L7ConversationFilterPredicate)
                  .ConfigureAwait(false);
      }
      else
      {
        l7Conversations = await this._l7ConversationFacade.GetAllForProcessingAsync().ConfigureAwait(false);
      }

      return l7Conversations;
    }
  }
}
