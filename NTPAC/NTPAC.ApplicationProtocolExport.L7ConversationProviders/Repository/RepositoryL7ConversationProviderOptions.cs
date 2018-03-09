using System;
using System.Linq.Expressions;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ApplicationProtocolExport.L7ConversationProviders.Repository
{
  public class RepositoryL7ConversationProviderOptions
  {
    public Expression<Func<IL7ConversationEntity, Boolean>> L7ConversationFilterPredicate { get; set; }
  }
}
