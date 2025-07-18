using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Entity;

namespace Intellidesk.Data.Repositories
{
    /// <summary> Custom generic repository </summary>
    /// <typeparam name="TEntity">entity type</typeparam>
    /// <typeparam name="TKey">entity id type</typeparam>
    public abstract class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
        //IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly DbContext _dbContext = null;

        public DbContext Context { get { return _dbContext; } }

        public BaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Create/Add
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            var ent = await Task.FromResult(_dbContext.Set<TEntity>().Add(entity));
            return await Task.FromResult(ent);
        }
        public TEntity Create(TEntity entity)
        {
            return _dbContext.Set<TEntity>().Add(entity);
        }
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Task.FromResult(_dbContext.Set<TEntity>().AddRange(entities));
        }
        public void AddRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
        }
        #endregion

        #region Get/Read
        /// <summary> IQueryable GetAll </summary>
        /// <returns>Query</returns>
        public IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().AsQueryable();
        }
        public virtual IEnumerable<TEntity> Get(Func<TEntity, bool> exp)
        {
            return _dbContext.Set<TEntity>().Where(exp);
        }
        public virtual async Task<List<TEntity>> GetAsync()
        {
            return await _dbContext.Set<TEntity>().ToListAsync();
        }
        public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> exp)
        {
            return await _dbContext.Set<TEntity>().Where(exp).ToListAsync();
        }
        #endregion

        #region Find/Read
        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbContext.Set<TEntity>().Find(keyValues);
        }
        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbContext.Set<TEntity>().FindAsync(keyValues);
        }
        public virtual TEntity Find(Expression<Func<TEntity, bool>> exp)
        {
            return _dbContext.Set<TEntity>().FirstOrDefault(exp);
        }
        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> exp)
        {
            return await _dbContext.Set<TEntity>().FirstOrDefaultAsync(exp);
        }
        #endregion

        #region Update
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(entity);
        }
        public virtual TEntity Update(TEntity entity)
        {
            return UpdateAsync(entity).Result;
        }
        #endregion

        #region Delete 
        public void Delete(TKey id)
        {
            var entity = Find(id);
            _dbContext.Set<TEntity>().Remove(entity);
        }
        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }
        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }
        #endregion

        public IQueryable BuildQueryByKeys()
        {
            return null;
        }
    }
}
