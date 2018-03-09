using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.DTO.ConversationTracking;

namespace NTPAC.Persistence.Interfaces
{
  public interface IL7ConversationFacade
  {
    Task Delete(Guid id);
    Task<IEnumerable<L7ConversationListDTO>> GetAllAsync();
    Task<IEnumerable<L7ConversationListDTO>> GetByCaptureIdAsync(Guid captureId);
    Task<L7ConversationDetailDTO> GetByIdAsync(Guid id);
    Task<IEnumerable<L7ConversationProcessingDTO>> GetAllWhereForProcessingAsync(Expression<Func<IL7ConversationEntity, Boolean>> predicate);
    Task<IEnumerable<L7ConversationProcessingDTO>> GetAllForProcessingAsync();
    Task InsertAsync(L7Conversation l7Conversation);
  }
}
