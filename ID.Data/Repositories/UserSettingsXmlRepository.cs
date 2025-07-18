using System.Collections.Generic;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.Data.Repositories
{
    public interface IUserStatesXmlRepository
    {
        //IEnumerable<State> GetStates();
        //void SetStates(IEnumerable<State> states);
        void SetUpdate<T>(T entity, params string[] properties) where T : class;
        void SetUpdate<T>(T entity, string propertyName, object value) where T : class;
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    //[Export(typeof(IUserSettingRepository))]
    public class UserSettingsXmlRepository : XmlRepository<UserSetting>, IUserStatesXmlRepository
    {
        public UserSettingsXmlRepository(string xmlfileName, IUnitOfWorkAsync uow)
            : base(xmlfileName, uow)
        {
        }

        public IEnumerable<UserSetting> GetAll()
        {
            return new List<UserSetting>() {  }; //UserSetting.Load(XmlContext)
        }

        public void Save(IEnumerable<UserSetting> items)
        {
            //tems.ToList()[0].Save();
        }

        public void SetUpdate<T>(T entity, params string[] properties) where T : class
        {
            throw new System.NotImplementedException();
        }

        public void SetUpdate<T>(T entity, string propertyName, object value) where T : class
        {
            throw new System.NotImplementedException();
        }
    }

}
