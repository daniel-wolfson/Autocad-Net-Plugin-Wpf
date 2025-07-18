using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AcadNet.Data.Repositories.Infrastructure;

namespace AcadNet.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, IObjectState
    {
        void Insert(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities);
        void InsertOrUpdateGraph(TEntity entity);
        void InsertGraphRange(IEnumerable<TEntity> entities);
        void Delete(object id);
        void Delete(TEntity entity);
        void Update(TEntity entity);
        void Update(TEntity entity, string propertyName, object value);
        void UpdateRange(IEnumerable<TEntity> entities);
        
        TEntity Find(params object[] keyValues);
        IRepository<T> GetRepository<T>() where T : class, IObjectState;

        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        IQueryFluent<TEntity> Query();
        IQueryable<TEntity> Queryable();
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);

    }
}