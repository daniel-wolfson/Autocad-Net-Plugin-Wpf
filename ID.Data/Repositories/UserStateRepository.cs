using System.Collections.Generic;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;

namespace Intellidesk.Data.Repositories
{
    public interface IUserStateRepository
    {
        IEnumerable<Filter> GetUserStates();
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    //[Export(typeof(IUserSettingRepository))]
    public class FilterRepository : Repository<Filter>, IUserStateRepository
    {
        public FilterRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

        public IEnumerable<Filter> GetUserStates()
        {
            return base.Queryable();
        }

    }

}
