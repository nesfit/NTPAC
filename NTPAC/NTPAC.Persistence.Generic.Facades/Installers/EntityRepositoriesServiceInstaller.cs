using System;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.SnooperExportEntities;
using UnitOfWork;
using UnitOfWork.BaseDataEntity;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Generic.Facades.Installers
{
  public static class EntityRepositoriesServiceInstaller
  {
    public static void InstallUnitOfWork(IServiceCollection serviceCollection, Type unitOfWorkImplementationType)
    {
      // TODO param check
      serviceCollection.AddSingleton(typeof(IUnitOfWork), unitOfWorkImplementationType);
    }

    public static void InstallEntityRepositories(IServiceCollection serviceCollection, Type repositoryImplementationType)
    {
      // TODO repositoryType check
//      if (!typeof(IRepository<TEntity>).IsAssignableFrom(repositoryType))
//      {
//        throw new ArgumentException("Repository does not inherit IRepository<>");
//      }

      InstallEntityRepository<CaptureEntity>(serviceCollection, repositoryImplementationType);
      InstallEntityRepository<L7ConversationEntity>(serviceCollection, repositoryImplementationType);
      InstallEntityRepository<L7ConversationPdusShardEntity>(serviceCollection, repositoryImplementationType);
      
      InstallEntityRepository<HttpExportEntity>(serviceCollection, repositoryImplementationType);
      InstallEntityRepository<DnsExportEntity>(serviceCollection, repositoryImplementationType);
      InstallEntityRepository<GenericExportEntity>(serviceCollection, repositoryImplementationType);
    }
    
    private static void InstallEntityRepository<TEntity>(IServiceCollection serviceCollection, Type repositoryType) where TEntity: class, IDataEntity
    {
      serviceCollection.AddSingleton(typeof(IRepository<>).MakeGenericType(typeof(TEntity)), repositoryType.MakeGenericType(typeof(TEntity)));
            
      serviceCollection.AddSingleton<IRepositoryWriterAsync<TEntity>>(
        x => x.GetRequiredService<IRepository<TEntity>>());
      
      serviceCollection.AddSingleton<IRepositoryReaderAsync<TEntity>>(
        x => x.GetRequiredService<IRepository<TEntity>>());
    } 
  }
}