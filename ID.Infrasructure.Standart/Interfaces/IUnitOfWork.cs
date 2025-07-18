using System;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Interfaces
{
    public interface IUnitOfWork1 : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}