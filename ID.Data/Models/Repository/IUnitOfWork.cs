using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadNet.Data.Repository
{
    public interface IUnitOfWork1 : IDisposable
    {

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        int Commit();
    }

    public interface IUnitOfWork : IDisposable
{
    IDbSet<TEntity> Set<TEntity>();
    int Save();
}

}
