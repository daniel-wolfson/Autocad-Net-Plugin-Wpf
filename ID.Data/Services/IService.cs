﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Services
{
    public interface IService<TEntity> where TEntity : IObjectState
    {
        TEntity Find(params object[] keyValues);
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        void Insert(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities);
        void InsertOrUpdateGraph(TEntity entity);
        void InsertGraphRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entity);
        IQueryFluent<TEntity> Query();
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        //Task<TEntity> FindAsync(params object[] keyValues);
        //Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        //Task<bool> DeleteAsync(params object[] keyValues);
        //Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);
        IQueryable<TEntity> Queryable();
    }
}
