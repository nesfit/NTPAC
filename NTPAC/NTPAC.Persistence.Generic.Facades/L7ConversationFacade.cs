using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nelibur.ObjectMapper;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.DTO.ConversationTracking;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.Mappers;
using NTPAC.Persistence.Generic.Facades.Converters;
using NTPAC.Persistence.Generic.Facades.Extensions;
using NTPAC.Persistence.Generic.Facades.Sharders;
using NTPAC.Persistence.Interfaces;
using UnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Generic.Facades
{
  
  public class L7ConversationFacade : IL7ConversationFacade
  {
    private readonly IRepositoryReaderAsync<L7ConversationEntity> _repositoryReaderAsync;
    private readonly IRepositoryWriterAsync<L7ConversationEntity> _repositoryWriterAsync;
    private readonly IRepositoryReaderAsync<L7ConversationPdusShardEntity> _pduShardRepositoryReaderAsync;
    private readonly IRepositoryWriterAsync<L7ConversationPdusShardEntity> _pduShardRepositoryWriterAsync;
    private readonly IUnitOfWork _unitOfWork;

    public L7ConversationFacade(IUnitOfWork unitOfWork,
                                IRepositoryWriterAsync<L7ConversationEntity> repositoryWriterAsync,
                                IRepositoryReaderAsync<L7ConversationEntity> repositoryReaderAsync,
                                IRepositoryWriterAsync<L7ConversationPdusShardEntity> pduShardRepositoryWriterAsync,
                                IRepositoryReaderAsync<L7ConversationPdusShardEntity> pduShardRepositoryReaderAsync)
    {
      TypeDescriptor.AddAttributes(typeof(IPEndPointEntity), new TypeConverterAttribute(typeof(IPEndPointEntityConverter)));
      TinyMapper.Bind<L7ConversationEntity, L7ConversationListDTO>();
      TinyMapper.Bind<L7ConversationEntity, L7ConversationDetailDTO>();
      TinyMapper.Bind<L7ConversationEntity, L7ConversationProcessingDTO>();
      TinyMapper.Bind<L7PduEntity, L7PduDTO>();

      this._unitOfWork                    = unitOfWork;
      this._repositoryReaderAsync         = repositoryReaderAsync;
      this._repositoryWriterAsync         = repositoryWriterAsync;
      this._pduShardRepositoryReaderAsync = pduShardRepositoryReaderAsync;
      this._pduShardRepositoryWriterAsync = pduShardRepositoryWriterAsync;
    }

    public Task Delete(Guid id) => this._repositoryWriterAsync.DeleteAsync(id);

    public async Task<IEnumerable<L7ConversationListDTO>> GetAllAsync() =>
      (await this._repositoryReaderAsync.GetAllAsync().ConfigureAwait(false)).Select(TinyMapper.Map<L7ConversationListDTO>);

    public async Task<IEnumerable<L7ConversationListDTO>> GetByCaptureIdAsync(Guid captureId) =>
      (await this._repositoryReaderAsync
                 .GetAllWhereAsync(l7ConversationEntity => l7ConversationEntity.CaptureId.Equals(captureId))
                 .ConfigureAwait(false)).Select(TinyMapper.Map<L7ConversationListDTO>);

    public async Task<L7ConversationDetailDTO> GetByIdAsync(Guid id)
    {
      var l7ConversationEntity = await this._repositoryReaderAsync.GetByIdAsync(id).ConfigureAwait(false);
      if (l7ConversationEntity == null)
      {
        return null;
      }

      var l7ConversationDetailDto = TinyMapper.Map<L7ConversationDetailDTO>(l7ConversationEntity);

      var            pduShardNum = 0;
      List<L7PduDTO> pduDtos     = null;
      while (true)
      {
        var targetPduShardNum = pduShardNum;
        var pduShardEntity = await this._pduShardRepositoryReaderAsync
                                       .GetSingleWhereAsync(
                                         entity => entity.L7ConversationId == id && entity.Shard == targetPduShardNum)
                                       .ConfigureAwait(false);
        if (pduShardEntity == null)
        {
          break;
        }

        if (pduDtos == null)
        {
          pduDtos = new List<L7PduDTO>(pduShardEntity.Pdus.Length);
        }

        pduDtos.InsertRange(pduDtos.Count, pduShardEntity.Pdus.Select(TinyMapper.Map<L7PduDTO>));
        pduShardNum++;
      }

      l7ConversationDetailDto.Pdus = pduDtos ?? new List<L7PduDTO>();
      return l7ConversationDetailDto;
    }

    public async Task<IEnumerable<L7ConversationProcessingDTO>> GetAllWhereForProcessingAsync(
      Expression<Func<IL7ConversationEntity, Boolean>> predicate)
    {
      var convertedPredicate = predicate?.ConvertPredicateParameter<IL7ConversationEntity, L7ConversationEntity>();
    
      var l7ConversationEntities = convertedPredicate != null ?
        await this._repositoryReaderAsync.GetAllWhereAsync(convertedPredicate).ConfigureAwait(false) :
        await this._repositoryReaderAsync.GetAllAsync().ConfigureAwait(false);

      var l7ConversationDtos = new List<L7ConversationProcessingDTO>();

      foreach (var l7ConversationEntity in l7ConversationEntities)
      {
        var l7ConversationDto = TinyMapper.Map<L7ConversationProcessingDTO>(l7ConversationEntity);

        var            pduShardNum = 0;
        List<L7PduDTO> pduDtos     = null;
        while (true)
        {
          var targetPduShardNum = pduShardNum;
          var pduShardEntity = await this._pduShardRepositoryReaderAsync
                                         .GetSingleWhereAsync(
                                           entity => entity.L7ConversationId == l7ConversationEntity.Id &&
                                                     entity.Shard            == targetPduShardNum).ConfigureAwait(false);
          if (pduShardEntity == null)
          {
            break;
          }

          if (pduDtos == null)
          {
            pduDtos = new List<L7PduDTO>(pduShardEntity.Pdus.Length);
          }

          pduDtos.AddRange(pduShardEntity.Pdus.Select(TinyMapper.Map<L7PduDTO>));
          
          pduShardNum++;
        }

        l7ConversationDto.Pdus = pduDtos ?? new List<L7PduDTO>();

        l7ConversationDtos.Add(l7ConversationDto);
      }

      return l7ConversationDtos;
    }

    public Task<IEnumerable<L7ConversationProcessingDTO>> GetAllForProcessingAsync() => this.GetAllWhereForProcessingAsync(null);

    public async Task InsertAsync(L7Conversation l7Conversation)
    {
      await this._repositoryWriterAsync.InsertAsync(L7ConversationMapper.Map(l7Conversation)).ConfigureAwait(false);
      // TODO async ShardL7ConversationPdus 
      await Task.WhenAll(L7ConversationPdusSharder.ShardL7ConversationPdus(l7Conversation)
                                                  .Select(this._pduShardRepositoryWriterAsync.InsertAsync)).ConfigureAwait(false);
      
      await this._unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}
