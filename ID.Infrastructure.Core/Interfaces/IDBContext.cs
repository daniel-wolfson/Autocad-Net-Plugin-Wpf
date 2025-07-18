using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace ID.Infrastructure.Interfaces
{
    public interface IDbContext : IDisposable, IAsyncDisposable, IInfrastructure<IServiceProvider>, IDbContextDependencies, IDbSetCache, IDbContextPoolable, IResettableService
    {
        public ChangeTracker ChangeTracker { get; }
        public DatabaseFacade Database { get; }
        public DbContextId ContextId { get; }

        public EntityEntry Add(object entity);
        public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
        public ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default);
        public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        public void AddRange(params object[] entities);
        public void AddRange(IEnumerable<object> entities);
        public Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
        public Task AddRangeAsync(params object[] entities);
        public EntityEntry Attach(object entity);
        public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
        public void AttachRange(IEnumerable<object> entities);
        public void AttachRange(params object[] entities);
        public EntityEntry Entry(object entity);
        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        public bool Equals(object obj);
        public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class;
        public object Find(Type entityType, params object[] keyValues);
        public ValueTask<object> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken);
        public ValueTask<object> FindAsync(Type entityType, params object[] keyValues);
        public ValueTask<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class;
        public ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
        public int GetHashCode();
        public DbSet<TQuery> Query<TQuery>() where TQuery : class;
        public EntityEntry Remove(object entity);
        public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;
        public void RemoveRange(IEnumerable<object> entities);
        public void RemoveRange(params object[] entities);
        public int SaveChanges(bool acceptAllChangesOnSuccess);
        public int SaveChanges();
        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public DbSet<TEntity> Set<TEntity>() where TEntity : class;
        public string ToString();
        public EntityEntry Update(object entity);
        public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
        public void UpdateRange(IEnumerable<object> entities);
        public void UpdateRange(params object[] entities);
        protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
        protected void OnModelCreating(ModelBuilder modelBuilder);

        void ClearChangesCounter();
        void ClearGlobalContextId();
        int GetChangesCounter();

        //int SaveChanges();
        //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}