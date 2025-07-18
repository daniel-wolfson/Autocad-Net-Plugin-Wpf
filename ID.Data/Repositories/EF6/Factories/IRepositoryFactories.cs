using System;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories.EF6.Factories
{
    public interface IRepositoryFactories
    {
        Func<IDataContextAsync, IUnitOfWorkAsync, dynamic> GetRepositoryFactory<T>();
        Func<IDataContextAsync, IUnitOfWorkAsync, dynamic> GetRepositoryFactoryForEntityType<T>() where T : class, 
            IObjectState;
    }
}
