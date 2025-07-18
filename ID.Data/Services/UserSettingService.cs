using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.Data.Services
{
    public interface IUserSettingService : IService<User>
    {
        IEnumerable<UserSetting> GetUserSettings();
        ObservableCollection<UserSetting> GetItems();
    }

    public class UserSettingService : Service<User>, IUserSettingService
    {
        private IEnumerable<UserSetting> _items;

        public UserSettingService(IUnitOfWorkAsync uow)
            : base(uow.Repository<User>())
        {
            var repository = uow.Repository<User>();
            var contextItems = repository.Queryable().Select(x => x.Settings_Data).ToList();
            _items = new List<UserSetting>();
            foreach (var innerList in contextItems.Select(item => item.XParse<UserSetting>()))
            {
                ((List<UserSetting>)_items).AddRange(innerList);
            }
        }

        public UserSettingService(IUnitOfWorkAsync uow, string xmlFileName)
            : base(xmlFileName)
        {
            if (xmlFileName == "")
                xmlFileName = Assembly.GetExecutingAssembly().GetName().Name + "." + typeof(UserSetting).Name;

            _items = (new XmlRepository<UserSetting>(xmlFileName, uow)).Load();
        }


        public IEnumerable<UserSetting> GetUserSettings()
        {
            return _items ?? (_items = GetDemoData());
        }

        public ObservableCollection<UserSetting> GetItems()
        {
            return GetUserSettings().ToItems();
        }

        public IEnumerable<UserSetting> GetDemoData()
        {
            return new List<UserSetting> { new UserSetting()
            {
                UserSettingId = 0, ChainDistance = 2, ConfigSetName = "Default", DateStarted = DateTime.Now, Drive = "C:", MinWidth = 375
            }};
        }
    }
}