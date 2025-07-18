using General.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace General.Interfaces
{
    public interface IBaseRepository
    {
        IDBContext DbContext { get; set; }
        IBaseService Parent { get; set; }
    }

    public interface IRepository<TEntity, TKey> : IBaseRepository where TEntity : class
    {
        // Insert
        EntityEntry<TEntity> Add(TEntity entity);
        Task<EntityEntry<TEntity>> AddAsync(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Get
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null);
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, object>> orderBy = null,
            string[] includePaths = null);
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
           string[] includePaths = null,
           int? page = null, int? pageSize = null,
           params SortExpression<TEntity>[] sortExpressions);
        IQueryable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAsync();
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> exp,
            string[] includePaths = null,
            Expression<Func<TEntity, object>> orderBy = null);
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> exp,
            string[] includePaths = null,
            params SortExpression<TEntity>[] sortExpressions);

        // Delete
        EntityEntry<TEntity> Delete(TKey id);
        EntityEntry<TEntity> Delete(TEntity entity);
        Task<EntityEntry<TEntity>> DeleteAsync(TEntity entity);
        bool DeleteRange(IEnumerable<TEntity> entities);
        Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entities);

        // Find
        TEntity Find(Expression<Func<TEntity, bool>> exp);
        TEntity Find(params object[] keyValues);
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> exp);
        Task<TEntity> FindAsync(params object[] keyValues);

        // Update
        EntityEntry<TEntity> Update(TEntity entity);
        Task<EntityEntry<TEntity>> UpdateAsync(TEntity entity);

        List<KeyValuePair<TEntity, EntityState>> GetTrackEntities();
    }
}