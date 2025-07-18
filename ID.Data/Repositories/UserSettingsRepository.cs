using System.Collections.Generic;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.Data.Repositories
{
    public interface IUserSettingRepository
    {
        IEnumerable<UserSetting> GetUserSettings();
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    //[Export(typeof(IUserSettingRepository))]
    public class UserSettingsRepository : Repository<UserSetting>, IUserSettingRepository
    {
        public UserSettingsRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

        public IEnumerable<UserSetting> GetUserSettings()
        {
            return base.Queryable();
        }

    }
}
