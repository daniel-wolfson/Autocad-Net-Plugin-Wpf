using System.Configuration;
using System.ServiceModel.Configuration;

namespace Intellidesk.Data.Services
{
    public static class ConfigurationExtensions
    {
        public static ServiceModelSectionGroup GetSectionGroup(this Configuration config)
        {
            return ServiceModelSectionGroup.GetSectionGroup(config);
        }
    }
}
