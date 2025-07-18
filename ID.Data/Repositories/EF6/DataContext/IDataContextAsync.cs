using System.Threading;
using System.Threading.Tasks;

namespace Intellidesk.Data.Repositories.EF6.DataContext
{
    public interface IDataContextAsync : IDataContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
    }
}