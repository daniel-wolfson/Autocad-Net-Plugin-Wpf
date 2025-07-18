using System.Collections.Generic;
using System.Linq;
using Intellidesk.AcadNet.Data.Models;
using Intellidesk.AcadNet.Data.Repositories.EF6.UnitOfWork;

namespace Intellidesk.AcadNet.Data.Repositories.Intel
{
    //[Export(typeof(IUserSettingRepository))]
    public class UserSettingsRepository : XmlRepository<UserSetting>
    {
        private readonly string _xmlfileName;
        public UserSettingsRepository(string xmlfileName, IUnitOfWorkAsync uow)
            : base(xmlfileName, uow)
        {
            _xmlfileName = xmlfileName;
        }

        public IEnumerable<UserSetting> GetUserSettings()
        {
            return new List<UserSetting>()
            {
                UserSetting.Load(_xmlfileName)
            };
        }

        public void SaveSettings(IEnumerable<UserSettingMetaData> userSettings)
        {
            userSettings.ToList()[0].Save();
        }
    }
}