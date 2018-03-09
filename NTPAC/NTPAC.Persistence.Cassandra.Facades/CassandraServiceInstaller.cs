using System;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.SnooperExportEntities;
using NTPAC.Persistence.Generic.Facades.Installers;
using UnitOfWork;
using UnitOfWork.CassandraRepository;
using UnitOfWork.CassandraUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Facades
{
  public static class CassandraServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection, String keyspace, params String[] contactPoints)
    {
      serviceCollection.AddSingleton<Mappings, CassandraMappings>();
      serviceCollection.AddSingleton<ICluster>(provider => Cluster.Builder().AddContactPoints(contactPoints).Build());

      serviceCollection.AddSingleton<IUnitOfWork>(provider =>
      {
        var unitOfWork = new CassandraUnitOfWork(provider.GetRequiredService<ICluster>(), provider.GetRequiredService<Mappings>(),
                                                 keyspace);

        // Map UDTs (just UDTs, not entities with their own tables)
        var session = unitOfWork.Session;
        session.UserDefinedTypes.Define(UdtMap.For<L7PduEntity>(),
                                        UdtMap.For<IPEndPointEntity>(),
                                        UdtMap.For<DnsExportEntity.DnsQueryEntity>(), 
                                        UdtMap.For<DnsExportEntity.DnsAnswerEntity>());

        return unitOfWork;
      });

      EntityRepositoriesServiceInstaller.InstallEntityRepositories(serviceCollection, typeof(BaseRepository<>));  
    }
  }
}
