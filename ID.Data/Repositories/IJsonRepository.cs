using System.Collections.Generic;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories
{
    public interface IJsonRepository<TEntity> : IRepository<TEntity> where TEntity : class, IObjectState
    {
        IEnumerable<TEntity> Load(string filename = "");
        void Save();
    }

    public interface IJsonRepositoryAsync<TEntity> : IJsonRepository<TEntity> where TEntity : class, IObjectState
    {
        IEnumerable<TEntity> LoadAsync(string filename = "");
        //void Save();
    }
}