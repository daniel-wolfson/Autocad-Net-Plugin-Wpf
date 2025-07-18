using ID.Infrastructure.Contexts;
using ID.Infrastructure.Core;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ID.Infrastructure.Repositories
{
    /// <summary> Custom generic repository </summary>
    /// <typeparam name="TEntity">entity type</typeparam>
    /// <typeparam name="TKey">entity id type</typeparam>
    public class GenericRepository<TDbContext, TEntity, TKey> : IRepository<TDbContext, TEntity, TKey>
        where TEntity : class where TDbContext : DbContext
    {
        private readonly DbSet<TEntity> dbSet;

        private IBaseService<TDbContext> _parent;
        public virtual IBaseService<TDbContext> Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                DbContext = value.DbContext;
            }
        }

        private DbContext _dbContext;
        public DbContext DbContext
        {
            get
            {
                _dbContext = GeneralContext.GetService<DbContext>();
                return _dbContext;
            }
            set
            {
                _dbContext = value;
            }
        }

        public GenericRepository()
        {
            dbSet = DbContext.Set<TEntity>();
        }

        #region Create/Add

        public Task<EntityEntry<TEntity>> AddAsync(TEntity entity)
        {
            this.AttachEnsured(entity);
            var task = dbSet.AddAsync(entity).AsTask().ContinueWith(t => t.Result);
            return task;
        }

        public EntityEntry<TEntity> Add(TEntity entity)
        {
            this.AttachEnsured(entity);
            var entry = dbSet.Add(entity);
            return entry;
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            this.AttachEnsured(entities);
            await dbSet.AddRangeAsync(entities);
        }
        public void AddRange(IEnumerable<TEntity> entities)
        {
            this.AttachEnsured(entities);
            dbSet.AddRange(entities);
        }
        #endregion

        #region Get/Read
        /// <summary> IQueryable GetAll </summary>
        /// <returns>Query</returns>
        public IQueryable<TEntity> GetAll()
        {
            return dbSet.AsQueryable();
        }

        public IQueryable<TEntity> Get()
        {
            return Get(null, null, null, null, null);
        }
        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null)
        {
            return Get(filter, null, null, null, null);
        }
        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, string[] includePaths = null)
        {
            return Get(filter, includePaths, null, null, null);
        }

        public virtual IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, object>> orderBy = null,
            string[] includePaths = null)
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (includePaths != null)
                query = AddQueryIncludePaths(query, includePaths);

            //foreach (var includeProperty in includeProperties.Split
            //    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    query = query.Include(includeProperty);
            //}

            if (orderBy != null)
                return query.OrderBy(orderBy);
            else
                return query;
        }

        public virtual IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            string[] includePaths = null,
            int? page = null, int? pageSize = null,
            params SortExpression<TEntity>[] sortExpressions) //Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
                query = query.Where(filter);

            if (includePaths != null)
                query = AddQueryIncludePaths(query, includePaths);

            if (sortExpressions != null)
                query = AddSortExpressions(query, sortExpressions);

            if (page != null)
                query = query.Skip(((int)page - 1) * (int)pageSize);

            if (pageSize != null)
                query = query.Take((int)pageSize);

            return query;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync()
        {
            return await dbSet.ToListAsync();
        }
        public virtual async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> exp,
            string[] includePaths = null,
            Expression<Func<TEntity, object>> orderBy = null)
        {
            var query = dbSet.AsQueryable();

            if (exp != null)
                query = query.Where(exp);
            if (includePaths != null)
                query = AddQueryIncludePaths(query, includePaths);

            if (orderBy != null)
                query = query.OrderBy(orderBy);

            return await query.ToListAsync();
        }
        public virtual async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> exp,
            string[] includePaths = null,
            params SortExpression<TEntity>[] sortExpressions)
        {
            var query = dbSet.AsQueryable();

            if (exp != null)
                query = query.Where(exp);

            if (includePaths != null)
                query = AddQueryIncludePaths(query, includePaths);

            if (sortExpressions != null)
                query = AddSortExpressions(query, sortExpressions);

            return await query.ToListAsync();
        }

        #endregion

        #region Find/Any
        public virtual TEntity Find(params object[] keyValues)
        {
            return dbSet.Find(keyValues);
        }
        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await dbSet.FindAsync(keyValues);
        }
        public virtual TEntity Find(Expression<Func<TEntity, bool>> exp)
        {
            return dbSet.FirstOrDefault(exp);
        }
        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> exp)
        {
            return await dbSet.FirstOrDefaultAsync(exp);
        }
        #endregion

        #region Update
        public Task<EntityEntry<TEntity>> UpdateAsync(TEntity entity)
        {
            this.AttachEnsured(entity);
            return Task.FromResult(Update(entity));
        }
        public virtual EntityEntry<TEntity> Update(TEntity entity)
        {
            this.AttachEnsured(entity);

            var entry = dbSet.Update(entity);
            return entry;
        }
        #endregion

        #region Delete 
        public EntityEntry<TEntity> Delete(TKey id)
        {
            var entity = Find(id);
            this.AttachEnsured(entity);
            return Delete(entity);
        }
        public EntityEntry<TEntity> Delete(TEntity entity)
        {
            this.AttachEnsured(entity);
            return dbSet.Remove(entity);
        }
        public Task<EntityEntry<TEntity>> DeleteAsync(TEntity entity)
        {
            return Task.FromResult(Delete(entity));
        }
        public bool DeleteRange(IEnumerable<TEntity> entities)
        {
            this.AttachEnsured(entities);
            dbSet.RemoveRange(entities);
            return true;
        }
        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            DeleteRange(entities);
            return await Task.FromResult(true);
        }
        #endregion

        #region Other methods

        public int Count()
        {
            return dbSet.AsQueryable().Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return dbSet.AsQueryable().Count(predicate);
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> exp)
        {
            return dbSet.Any(exp);
        }
        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> exp)
        {
            return dbSet.AnyAsync(exp);
        }

        public List<KeyValuePair<TEntity, EntityState>> GetTrackEntities()
        {
            var ents = DbContext.GetTrackEntries<TEntity>().ToList();
            return ents;
        }

        public IQueryable BuildQueryByKeys()
        {
            return null;
        }

        public virtual TEntity CreateInstanceByDefault()
        {
            return default;
        }

        // TODO: Paging
        //private IQueryable<TEntity> GetPage(int pageIndex = 0, int pageSize = 10, string sortBy = "ID", string sortDirection = "asc")
        //{
        //    var param = Expression.Parameter(typeof(TEntity));
        //    var sortExpression = Expression.Lambda<Func<TEntity, object>>
        //        (Expression.Convert(Expression.Property(param, sortBy), typeof(object)), param);
        //    var results = dbSet.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        //    switch (sortDirection.ToLower())
        //    {
        //        case "asc":
        //            return results.OrderBy(sortExpression);
        //        default:
        //            return results.OrderByDescending(sortExpression);
        //    }
        //}

        #endregion Other methods

        #region Privates

        /// <summary> Detached entity in local context </summary>
        private void AttachEnsured(TEntity entity)
        {
            // get entity Keys
            var entityUniqueKeys = dbSet.GetKeys().ToList();
            Type entityType = typeof(TEntity);

            // existing entities in local context
            var localEntities = DbContext.Set<TEntity>().Local.Where(localEntity =>
            {
                bool compareResult = entityUniqueKeys.All(pi =>
                {
                    object localValue = entityType.GetProperty(pi.Name).GetValue(localEntity, null);
                    object inputValue = entityType.GetProperty(pi.Name).GetValue(entity, null);
                    return localValue != null && (localValue == inputValue || localValue.Equals(inputValue));
                });
                return compareResult;
            }).ToList();

            // Detach exist entities
            if (localEntities.Any())
                localEntities.ForEach(p => DbContext.Entry(p).State = EntityState.Detached);

            // Attach the detached entity
            if (DbContext.Entry(entity).State == EntityState.Detached)
                dbSet.Attach(entity);
        }

        private void AttachEnsured(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                AttachEnsured(entity);
        }

        private IQueryable<TEntity> AddQueryIncludePaths(IQueryable<TEntity> query, string[] includePaths)
        {
            if (includePaths != null)
                for (var i = 0; i < includePaths.Count(); i++)
                {
                    query = query.Include(includePaths[i]);
                }
            return query;
        }
        private IQueryable<TEntity> AddSortExpressions(IQueryable<TEntity> query, SortExpression<TEntity>[] sortExpressions)
        {
            if (sortExpressions != null)
            {
                IOrderedQueryable<TEntity> orderedQuery = null;
                for (var i = 0; i < sortExpressions.Count(); i++)
                {
                    if (i == 0)
                    {
                        if (sortExpressions[i].SortDirection == ListSortDirection.Ascending)
                            orderedQuery = query.OrderBy(sortExpressions[i].SortBy);
                        else
                            orderedQuery = query.OrderByDescending(sortExpressions[i].SortBy);
                    }
                    else
                    {
                        if (sortExpressions[i].SortDirection == ListSortDirection.Ascending)
                            orderedQuery = orderedQuery.ThenBy(sortExpressions[i].SortBy);
                        else
                            orderedQuery = orderedQuery.ThenByDescending(sortExpressions[i].SortBy);

                    }
                }
                query = orderedQuery;
            }
            return query;
        }

        #endregion Privates
    }
}
