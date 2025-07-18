using System.Threading;
using System.Threading.Tasks;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories.EF6.UnitOfWork
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState;

        IXmlRepositoryAsync<TEntity> XmlRepositoryAsync<TEntity>() where TEntity : class, IObjectState;

        IJsonRepositoryAsync<TEntity> JsonRepositoryAsync<TEntity>() where TEntity : class, IObjectState;
    }
}