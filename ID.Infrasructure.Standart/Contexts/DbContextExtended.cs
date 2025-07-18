using ID.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Contexts
{
    public partial class GeneralDBContext : DbContext, IDBContext
    {
        bool disposed = false;
        private static int _globalContextId;
        [NotMapped]
        /// <summary> auto increment number/order instnace of context /// </summary>
        public readonly int GlobalContextId = Interlocked.Increment(ref _globalContextId);

        /// <summary> total count of all savechanges within single transaction /// </summary>
        private int _changesCounter;
        public int GetChangesCounter()
        { return _changesCounter; }
        public void ClearChangesCounter()
        { _changesCounter = 0; }

        public static readonly ILoggerFactory ConsoleLoggerFactory = LoggerFactory.Create(builder =>
           {
               builder.AddFilter("Microsoft", LogLevel.Warning)
                      .AddFilter("System", LogLevel.Warning)
                      .AddFilter("DbLoggerCategory.Database.Command.Name", LogLevel.Information)
                      .AddConsole();
           }
        );

        DatabaseFacade IDBContext.Database => throw new NotImplementedException();

        ChangeTracker IDBContext.ChangeTracker => throw new NotImplementedException();

        public override int SaveChanges()
        {
            //Log.Logger.Warning("Not use strange calling of SaveChanges from context, use the SaveChanges of service");
            return _changesCounter += base.SaveChanges();
        }

        protected void CustomModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<RolePermissions>(entity =>
            //{
            //entity.HasKey(e => new { e.RoleId, e.TabId })
            //    .HasName("tabroles_pkey");

            //entity.Property(e => e.TabId);

            //entity.Property(e => e.RoleId);

            //entity
            //    .HasOne(d => d)
            //    .WithMany(p => p.RoleId)
            //    .HasForeignKey(d => d.RoleId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("UserRoles_RoleId_fkey");

            //entity.HasOne(d => d.User)
            //    .WithMany(p => p.UserRoles)
            //    .HasForeignKey(d => d.UserId)
            //    .HasConstraintName("UserRoles_UserId_fkey");
            //});
        }

        public override void Dispose()
        {
            if (disposed)
                return;

            base.Dispose();
            disposed = true;
        }

        EntityEntry<TEntity> IDBContext.Add<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        Task<EntityEntry> IDBContext.AddAsync(object entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<EntityEntry<TEntity>> IDBContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDBContext.AddRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        void IDBContext.AddRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        Task IDBContext.AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IDBContext.AddRangeAsync(params object[] entities)
        {
            throw new NotImplementedException();
        }

        EntityEntry<TEntity> IDBContext.Attach<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        EntityEntry IDBContext.Attach(object entity)
        {
            throw new NotImplementedException();
        }

        void IDBContext.AttachRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        void IDBContext.AttachRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        EntityEntry<TEntity> IDBContext.Entry<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        EntityEntry IDBContext.Entry(object entity)
        {
            throw new NotImplementedException();
        }

        bool IDBContext.Equals(object obj)
        {
            throw new NotImplementedException();
        }

        object IDBContext.Find(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        TEntity IDBContext.Find<TEntity>(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        Task<TEntity> IDBContext.FindAsync<TEntity>(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        Task<object> IDBContext.FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TEntity> IDBContext.FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<object> IDBContext.FindAsync(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        int IDBContext.GetHashCode()
        {
            throw new NotImplementedException();
        }

        DbSet<TQuery> IDBContext.Query<TQuery>()
        {
            throw new NotImplementedException();
        }

        EntityEntry IDBContext.Remove(object entity)
        {
            throw new NotImplementedException();
        }

        EntityEntry<TEntity> IDBContext.Remove<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        void IDBContext.RemoveRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        void IDBContext.RemoveRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        int IDBContext.SaveChanges(bool acceptAllChangesOnSuccess)
        {
            throw new NotImplementedException();
        }

        int IDBContext.SaveChanges()
        {
            throw new NotImplementedException();
        }

        Task<int> IDBContext.SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<int> IDBContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        DbSet<TEntity> IDBContext.Set<TEntity>()
        {
            throw new NotImplementedException();
        }

        string IDBContext.ToString()
        {
            throw new NotImplementedException();
        }

        EntityEntry IDBContext.Update(object entity)
        {
            throw new NotImplementedException();
        }

        EntityEntry<TEntity> IDBContext.Update<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        void IDBContext.UpdateRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        void IDBContext.UpdateRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        void IDBContext.ClearChangesCounter()
        {
            throw new NotImplementedException();
        }

        int IDBContext.GetChangesCounter()
        {
            throw new NotImplementedException();
        }
    }

    //public partial class Users : IUsers
    //{
    //    //public ICollection<RolePermissions> RolePermissions { get; set; }
    //}

    //public partial class RolePermissions
    //{
    //    //public ICollection<Roles> Roles { get; set; }
    //}

    //public partial class Roles : IRoles
    //{
    //}
}