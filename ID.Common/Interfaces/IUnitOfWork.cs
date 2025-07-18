using System;
using System.Threading;
using System.Threading.Tasks;

namespace General.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}