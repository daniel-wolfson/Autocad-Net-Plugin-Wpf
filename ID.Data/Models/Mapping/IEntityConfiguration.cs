using System.Data.Entity.ModelConfiguration.Configuration;

namespace Intellidesk.Data.Models.Mapping
{
    public interface IEntityConfiguration
    {
        void AddConfiguration(ConfigurationRegistrar registrar);
    }

}
