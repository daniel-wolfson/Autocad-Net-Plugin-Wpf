using ID.Infrastructure.Interfaces;
using System.Collections.Generic;

namespace ID.Infrastructure.Models
{
    public class AppConfig : IAppConfig
    {
        public string Domain { get; set; }
        public Dictionary<string, string> Endpoints { get; set; }
        public string Version { get; set; }
        public string DefaultCultureName { get; set; }
        public string GroupName { get; set; }
        public string ResponseTimeout { get; set; }
    }
}
