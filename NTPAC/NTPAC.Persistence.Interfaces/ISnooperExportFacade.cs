using System.Threading.Tasks;
using NTPAC.ApplicationProtocolExport.Core.Models;

namespace NTPAC.Persistence.Interfaces
{
  public interface ISnooperExportFacade
  {
    Task InsertAsync(SnooperExportCollection snooperExportCollection);
  }
}
