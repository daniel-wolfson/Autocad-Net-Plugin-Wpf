using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.Data.Services
{
    public interface ISettingsService : IService<Settings>
    {
    }

    public class SettingsService : Service<Settings>, ISettingsService
    {
        public SettingsService(IRepository<Settings> repository)
            : base(repository)
        {
        }
    }
}