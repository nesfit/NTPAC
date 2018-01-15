using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NTPAC.Common.Interfaces.Persistence;

namespace NTPAC.Persistence
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public TEntity Get(Int32 id) => throw new NotImplementedException();

        public IEnumerable<TEntity> GetAll() => throw new NotImplementedException();

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, Boolean>> predicate) => throw new NotImplementedException();

        public TEntity SingleOrDefault(Expression<Func<TEntity, Boolean>> predicate) => throw new NotImplementedException();

        public void Add(TEntity entity) { throw new NotImplementedException(); }

        public void AddRange(IEnumerable<TEntity> entities) { throw new NotImplementedException(); }

        public void Remove(TEntity entity) { throw new NotImplementedException(); }

        public void RemoveRange(IEnumerable<TEntity> entities) { throw new NotImplementedException(); }
    }
}