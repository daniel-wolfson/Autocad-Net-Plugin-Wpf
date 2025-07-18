using System.Collections.Generic;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories
{
    public interface IXmlRepository<TEntity> : IRepository<TEntity> where TEntity : class, IObjectState
    {
        IEnumerable<TEntity> Load(string filename = "");
        void Save();
    }

    public interface IXmlRepositoryAsync<TEntity> : IXmlRepository<TEntity> where TEntity : class, IObjectState
    {
        IEnumerable<TEntity> LoadAsync(string filename = "");
        //void Save();
    }
}
