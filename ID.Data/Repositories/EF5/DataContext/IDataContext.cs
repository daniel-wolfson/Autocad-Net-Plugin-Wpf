using System;
using AcadNet.Data.Repositories.Infrastructure;

namespace AcadNet.Data.Repositories.EF5.DataContext
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        void SyncObjectState<TEntity>(TEntity entity) where TEntity : class, IObjectState;
        void SyncObjectsStatePostCommit();
    }
}