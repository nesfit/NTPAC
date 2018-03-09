using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.DTO.ConversationTracking;

namespace NTPAC.Persistence.Interfaces
{
  public interface ICaptureFacade
  {
    Task DeleteAsync(Guid id);
    Task<IEnumerable<CaptureListDTO>> GetAllAsync();
    Task<CaptureDetailDTO> GetByIdAsync(Guid id);
    Task InsertAsync(Capture capture);
    Task UpdateAsync(Capture capture);
  }
}
