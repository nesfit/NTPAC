using System;
using System.Linq;
using System.Threading.Tasks;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.Persistence.Entities.Mappers;
using NTPAC.Persistence.Entities.SnooperExportEntities;
using NTPAC.Persistence.Interfaces;
using UnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Generic.Facades
{
  public class SnooperExportFacade : ISnooperExportFacade
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepositoryWriterAsync<HttpExportEntity> _httpExportEntityWriter;
    private readonly IRepositoryWriterAsync<DnsExportEntity> _dnsExportEntityWriter;
    private readonly IRepositoryWriterAsync<GenericExportEntity> _genericExportEntityWriter;
    
    public SnooperExportFacade(IUnitOfWork unitOfWork,
                               IRepositoryWriterAsync<HttpExportEntity> httpExportEntityWriter,
                               IRepositoryWriterAsync<DnsExportEntity> _dnsExportEntityWriter,
                               IRepositoryWriterAsync<GenericExportEntity> genericExportEntityWriter)
    {
    
      this._unitOfWork = unitOfWork;
      this._httpExportEntityWriter = httpExportEntityWriter;
      this._dnsExportEntityWriter = _dnsExportEntityWriter;
      this._genericExportEntityWriter = genericExportEntityWriter;
    }

    public async Task InsertAsync(SnooperExportCollection snooperExportCollection)
    {
      var snooperExportEntities = SnooperExportCollectionMapper.Map(snooperExportCollection);
      var insertTasks = snooperExportEntities.Select(this.CreateInsertAsyncTask).ToArray();
      await Task.WhenAll(insertTasks).ConfigureAwait(false);
      await this._unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }

    private Task CreateInsertAsyncTask(SnooperExportEntityBase exportEntityBase)
    {
      switch (exportEntityBase)
      {
        case HttpExportEntity httpExportEntity:
          return this._httpExportEntityWriter.InsertAsync(httpExportEntity);
        case DnsExportEntity dnsExportEntity:
          return this._dnsExportEntityWriter.InsertAsync(dnsExportEntity);
        case GenericExportEntity genericExportEntity:
          return this._genericExportEntityWriter.InsertAsync(genericExportEntity);
        default:
          throw new ArgumentException("Invalid export entity type", nameof(exportEntityBase));
      }
    }
  }
}
