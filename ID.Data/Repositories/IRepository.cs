using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Intellidesk.Data.Repositories
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        void AddRange(IEnumerable<TEntity> entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        TEntity Create(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity);
        void Delete(TKey id);
        void DeleteRange(IEnumerable<TEntity> entities);
        TEntity Find(Expression<Func<TEntity, bool>> exp);
        TEntity Find(params object[] keyValues);
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> exp);
        Task<TEntity> FindAsync(params object[] keyValues);
        IEnumerable<TEntity> Get(Func<TEntity, bool> exp);
        IQueryable<TEntity> GetAll();
        Task<List<TEntity>> GetAsync();
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> exp);
        TEntity Update(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
    }
}