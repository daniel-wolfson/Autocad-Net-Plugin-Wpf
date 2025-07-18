using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories.EF6.DataContext
{
    public interface IDbContextBase
    {
        EntityState GetState<TEntity>(TEntity entity) where TEntity : class, IObjectState;
        IEnumerable<TEntity> FindByState<TEntity>(Func<DbEntityEntry<TEntity>, bool> predicate) where TEntity: class, IObjectState;
    }
}