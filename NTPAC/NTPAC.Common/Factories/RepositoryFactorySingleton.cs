using System;
using BaseDataEntity;
using Cassandra;
using Cassandra.Mapping;
using NTPAC.Common.Interfaces;
using Repository;
using UnitOfWork;

namespace NTPAC.Common.Factories
{
    public class RepositoryFactorySingleton
    {
        private static RepositoryFactorySingleton _instance;
        private readonly IRepositoryFactory _repositoryFactory;

        private RepositoryFactorySingleton(IRepositoryFactory repositoryFactory) => this._repositoryFactory = repositoryFactory;

        public static RepositoryFactorySingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException("RepositoryFactorySingleton is not initialized");
                }

                return _instance;
            }
        }

        public IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, IDataEntity, new() =>
            this._repositoryFactory.CreateRepository<TEntity>(unitOfWork);

        public IUnitOfWork CreateUnitOfWork() => this._repositoryFactory.CreateUnitOfWork();

        public static void InitForCassandra(ICluster cluster, Mappings mappings)
        {
            var cassandraRepositoryFactory = new CassandraRepositoryFactory(cluster, mappings);
            _instance = new RepositoryFactorySingleton(cassandraRepositoryFactory);
        }

        public static void InitForDevnull()
        {
            var devnullRepositoryFactory = new DevnullRepositoryFactory();
            _instance = new RepositoryFactorySingleton(devnullRepositoryFactory);
        }


        public static void InitForInMemory()
        {
            var inMemoryRepositoryFactory = new InMemoryRepositoryFactory();
            _instance = new RepositoryFactorySingleton(inMemoryRepositoryFactory);
        }

        public static void InitForInMemorySingle()
        {
            var inMemorySingleRepositoryFactory = new InMemorySingleRepositoryFactory();
            _instance = new RepositoryFactorySingleton(inMemorySingleRepositoryFactory);
        }
    }
}
