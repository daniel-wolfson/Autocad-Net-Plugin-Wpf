using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.DataContext
{
    public interface IUnitOfWork<T> : IUnitOfWork where T : DbContext
    {
    }

    public interface IUnitOfWork : IDisposable
    {
        string SessionId { get; }
        /// <summary>
        /// Returns the set of entities of given type
        /// </summary>
        DbSet<T> Set<T>() where T : class;

        /// <summary>
        /// Opens a new transaction instantly when being called.
        /// If a transaction is already open, it won't do anything.
        /// Generally, you shouldn't call this method unless you need
        /// to control the exact moment of opening a transaction.
        /// Unit of Work automatically handles opening transactions
        /// in a convenient time.        
        /// </summary>
        void ForceBeginTransaction();

        /// <summary>
        /// Commits the current transaction (does nothing when none exists).
        /// </summary>
        int CommitTransaction(CancellationToken cancellationToken = default);

        Task<int> CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction (does nothing when none exists).
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Saves changes to database, previously opening a transaction
        /// only when none exists. The transaction is opened with isolation
        /// level set in Unit of Work before calling this method.
        /// </summary>
        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the isolation level for new transactions.
        /// </summary>
        /// <param name="isolationLevel"></param>
        void SetIsolationLevel(IsolationLevel isolationLevel);
    }
}
