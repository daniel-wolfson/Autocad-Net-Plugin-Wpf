using System.Collections.Generic;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;

namespace Intellidesk.Data.Repositories
{
    public interface ILayourtFilterRepository : IRepository<Filter>
    {
        IEnumerable<Filter> GetAll();
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    //[Export(typeof(ILayoutFilterRepository))]
    public class LayourtFilterRepository : Repository<Filter>, ILayourtFilterRepository
    {
        public LayourtFilterRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

        public IEnumerable<Filter> GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}
