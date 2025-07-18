using System.Collections.Generic;

namespace ID.Infrastructure.DataContext
{
    public interface IUnitOfWorkPool
    {
        IEnumerable<string> RegisteredUoWKeys { get; }
        IUnitOfWork Get(string key);
        IEnumerable<IUnitOfWork> GetAll();
    }
}
