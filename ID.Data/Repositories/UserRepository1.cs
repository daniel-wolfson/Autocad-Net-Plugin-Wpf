using System.Collections.Generic;
using System.Linq;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using User = Intellidesk.Data.Models.Cad.User;

namespace Intellidesk.Data.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(string environmentUserName);
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    //[Export(typeof(IUserSettingRepository))]
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

        public User GetUser(string environmentUserName)
        {
            return base.Queryable().FirstOrDefault(x => x.Name == environmentUserName);
        }
        public IEnumerable<User> GetUsers()
        {
            return base.Queryable();
        }

    }
}
