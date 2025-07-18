using System;
using System.Data.Entity;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories.EF6.DataContext
{
    public interface IDataContext : IDisposable
    {
        Database Database { get; }
        int SaveChanges();
        void SyncObjectsStatePostCommit();
        void SyncObjectState<TEntity>(TEntity entity) where TEntity : class, IObjectState;
    }
}