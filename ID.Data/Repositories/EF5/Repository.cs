﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;


using LinqKit;
using AcadNet.Data.Repositories.Infrastructure;
using AcadNet.Data.Repositories.Ef5.UnitOfWork;
using AcadNet.Data.Repositories.EF5.DataContext;

namespace AcadNet.Data.Repositories.Ef5
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IObjectState
    {
        #region Private Fields

        private readonly string _xmlfileName;
        private readonly IDataContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IUnitOfWork _unitOfWork;

        #endregion Private Fields

        public Repository(IDataContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;

            // Temporarily for FakeDbContext, Unit Test and Fakes
            var dbContext = context as DbContext;

            if (dbContext != null)
            {
                _dbSet = dbContext.Set<TEntity>();
            }
            else
            {
                var fakeContext = context as FakeDbContext;

                if (fakeContext != null)
                {
                    _dbSet = fakeContext.Set<TEntity>();
                }
            }
        }

        public Repository(string xmlfileName)
        {
            _xmlfileName = xmlfileName;
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        public virtual void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            entity.ObjectState = ObjectState.Deleted;
            _dbSet.Attach(entity);
            _context.SyncObjectState(entity);
        }

        public virtual void Insert(TEntity entity)
        {
            entity.ObjectState = ObjectState.Added;
            _dbSet.Attach(entity);
            _context.SyncObjectState(entity);
        }

        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
        }

        public virtual void InsertOrUpdateGraph(TEntity entity)
        {
            SyncObjectGraph(entity);
            _dbSet.Add(entity);
        }

        public virtual void InsertGraphRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            entity.ObjectState = ObjectState.Modified;
            _dbSet.Attach(entity);
            _context.SyncObjectState(entity);
        }

        public virtual void Update(TEntity entity, string propertyName, object value)
        {
            entity.GetType().GetProperty(propertyName).SetValue(entity, value, null);
            Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            //throw new NotImplementedException();
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return _dbSet.SqlQuery(query, parameters).AsQueryable();
        }

        public IQueryFluent<TEntity> Query()
        {
            return new QueryFluent<TEntity>(this);
        }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject)
        {
            return new QueryFluent<TEntity>(this, queryObject);
        }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return new QueryFluent<TEntity>(this, query);
        }

        public IQueryable<TEntity> Queryable()
        {
            return _dbSet;
        }

        public virtual IRepository<T> GetRepository<T>() where T : class, IObjectState
        {
            return _unitOfWork.Repository<T>();
        }

        //public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        //{
        //    return await _dbSet.FindAsync(keyValues);
        //}

        //public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        //{
        //    return await _dbSet.FindAsync(cancellationToken, keyValues);
        //}

        //public virtual async Task<bool> DeleteAsync(params object[] keyValues)
        //{
        //    return await DeleteAsync(CancellationToken.None, keyValues);
        //}

        //public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        //{
        //    var entity = await FindAsync(cancellationToken, keyValues);

        //    if (entity == null)
        //    {
        //        return false;
        //    }

        //    entity.ObjectState = ObjectState.Deleted;
        //    _dbSet.Attach(entity);

        //    return true;
        //}

        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }
            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        //internal async Task<IEnumerable<TEntity>> SelectAsync(
        //    Expression<Func<TEntity, bool>> query = null,
        //    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        //    List<Expression<Func<TEntity, object>>> includes = null,
        //    int? page = null,
        //    int? pageSize = null)
        //{
        //    //See: Best Practices in Asynchronous Programming http://msdn.microsoft.com/en-us/magazine/jj991977.aspx
        //    return await Task.Run(() => Select(query, orderBy, includes, page, pageSize).AsEnumerable()).ConfigureAwait(false);
        //}

        private void SyncObjectGraph(object entity)
        {
            // Set tracking state for child collections
            foreach (var prop in entity.GetType().GetProperties())
            {
                // Apply changes to 1-1 and M-1 properties
                var trackableRef = prop.GetValue(entity, null) as IObjectState;
                if (trackableRef != null && trackableRef.ObjectState == ObjectState.Added)
                {
                    _dbSet.Attach((TEntity)entity);
                    _context.SyncObjectState((IObjectState)entity);
                }

                // Apply changes to 1-M properties
                var items = prop.GetValue(entity, null) as IList<IObjectState>;
                if (items == null) continue;

                foreach (var item in items)
                    SyncObjectGraph(item);
            }
        }
    }
}