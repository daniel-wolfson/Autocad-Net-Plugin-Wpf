using ID.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.DataContext
{
    /// <summary> UnitOfWork with single db context </summary>
    public class UnitOfWork<T> : IUnitOfWork<T> where T : IDbContext
    {
        protected readonly IDbContext Context;
        private IDbContextTransaction _transaction;
        private IsolationLevel? _isolationLevel;

        public string SessionId => throw new NotImplementedException();

        public UnitOfWork(T dbContext)
        {
            Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void StartTransaction()
        {
            if (_transaction == null)
            {
                //if (_isolationLevel.HasValue)
                //    _transaction = Context.Database.BeginTransaction(_isolationLevel.GetValueOrDefault());
                //else
                _transaction = Context.Database.BeginTransaction();
            }
        }

        public DbSet<Y> Set<Y>() where Y : class
        {
            return Context.Set<Y>();
        }

        public void ForceBeginTransaction()
        {
            StartTransaction();
        }

        public int CommitTransaction()
        {
            //do not open transaction here, because if during the request
            //nothing was changed (only select queries were run), we don't
            //want to open and commit an empty transaction - calling SaveChanges()
            //on _transactionProvider will not send any sql to database in such case
            var changeCount = Context.SaveChanges();

            if (_transaction != null)
            {
                _transaction.Commit();

                _transaction.Dispose();
                _transaction = null;
            }

            return changeCount;
        }

        public void RollbackTransaction()
        {
            if (_transaction == null) return;

            _transaction.Rollback();

            _transaction.Dispose();
            _transaction = null;
        }

        public int SaveChanges()
        {
            StartTransaction();
            return Context.SaveChanges();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StartTransaction();
            return Context.SaveChangesAsync();
        }

        public void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }

        public void Dispose()
        {
            if (_transaction != null)
                _transaction.Dispose();

            _transaction = null;
        }

        public int CommitTransaction(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public enum RefreshConflict
    {
        StoreWins,
        ClientWins,
        MergeClientAndStore
    }
}
