using System.Collections.Generic;
using System.Collections.ObjectModel;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Infrastructure;

namespace Intellidesk.Data.Services
{
    public interface IDataConfigService : IService<Config>
    {
        IEnumerable<Config> GetConfigs(bool fromCache = true);
        ObservableCollection<Config> GetItems();
    }
}