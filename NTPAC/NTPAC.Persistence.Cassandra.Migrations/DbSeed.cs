using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.Persistence.DTO.ConversationTracking;
using NTPAC.Persistence.Entities;
using UnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Migrations
{
  public class DbSeed : IDbSeed
  {
    private readonly IRepositoryWriterAsync<CaptureEntity> _captureRepositoryWriterAsync;
    private readonly IRepositoryWriterAsync<L7ConversationEntity> _l7ConversationRepositoryWriterAsync;
    private readonly IRepositoryWriterAsync<L7ConversationPdusShardEntity> _pduShardRepositoryWriterAsync;
    private readonly IUnitOfWork _unitOfWork;

    public DbSeed(IUnitOfWork unitOfWork,
                  IRepositoryWriterAsync<CaptureEntity> captureRepositoryWriterAsync,
                  IRepositoryWriterAsync<L7ConversationEntity> l7ConversationRepositoryWriterAsync,
                  IRepositoryWriterAsync<L7ConversationPdusShardEntity> pduShardRepositoryWriterAsync)
    {
      this._unitOfWork                          = unitOfWork;
      this._captureRepositoryWriterAsync        = captureRepositoryWriterAsync;
      this._l7ConversationRepositoryWriterAsync = l7ConversationRepositoryWriterAsync;
      this._pduShardRepositoryWriterAsync       = pduShardRepositoryWriterAsync;
    }

    public async Task SeedDb()
    {
      var capture = new CaptureEntity
                    {
                      Uri                = "smb://nesAtFit",
                      Id                 = Guid.Parse("28b1dfd6-0b66-4d73-90e5-30cb919a3290"),
                      Processed          = new DateTime(2018, 08, 07, 03, 15, 00)
                    };
      await this._captureRepositoryWriterAsync.InsertAsync(capture).ConfigureAwait(false);


      var l7Conversation = new L7ConversationEntity
                           {
                             Id                  = Guid.Parse("bb672537-7a79-4b47-bdeb-f46b6dc341f6"),
                             CaptureId           = capture.Id,
                             SourceEndPoint      = new IPEndPointEntity(IPAddress.Parse("147.229.10.10"), 10),
                             DestinationEndPoint = new IPEndPointEntity(IPAddress.Parse("8.8.8.8"), 20),
                             FirstSeen    = new DateTime(2018, 08, 07, 8, 30, 00),
                             LastSeen     = new DateTime(2018, 08, 07, 9, 0, 00),
                             ProtocolType = (Int32) IPProtocolType.TCP
                           };
      await this._l7ConversationRepositoryWriterAsync.InsertAsync(l7Conversation).ConfigureAwait(false);


      var pduShard = new L7ConversationPdusShardEntity
                 {
                   L7ConversationId = l7Conversation.Id,
                   Shard            = 0,
                   Pdus = new[]
                          {
                            new L7PduEntity
                            {
                              Direction      = (Int32) FlowDirection.Up,
                              FirstSeenTicks = new DateTime(2018, 08, 07, 8, 30, 00).Ticks,
                              Payload        = Encoding.ASCII.GetBytes("ABCD")
                            },
                            new L7PduEntity
                            {
                              Direction      = (Int32) FlowDirection.Down,
                              FirstSeenTicks = new DateTime(2018, 08, 07, 8, 45, 00).Ticks,
                              Payload        = Encoding.ASCII.GetBytes("EFGH")
                            },
                            new L7PduEntity
                            {
                              Direction      = (Int32) FlowDirection.Up,
                              FirstSeenTicks = new DateTime(2018, 08, 07, 9, 00, 00).Ticks,
                              Payload        = Encoding.ASCII.GetBytes("IJK")
                            }
                          },
                 };
      await this._pduShardRepositoryWriterAsync.InsertAsync(pduShard).ConfigureAwait(false);

      await this._unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}
