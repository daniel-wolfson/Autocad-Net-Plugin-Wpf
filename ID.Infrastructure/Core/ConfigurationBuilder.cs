using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ID.Infrastructure
{
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        public ConfigurationBuilder()
        {
        }

        public string WorkPath { get; set; }

        public bool IsDevelopmentEnvironment { get; set; } = true;

        public string DevelopmentEnvironment { get { return IsDevelopmentEnvironment ? "development" : "production"; } }

        private JObject _configuration { get; set; } = new JObject();

        public IConfigurationBuilder SetBasePath(string workPath)
        {
            this.WorkPath = workPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
            return this;
        }

        public IConfigurationBuilder AddJsonFile(string fileName, bool optional = true, bool reloadOnChange = true)
        {
            var json = File.ReadAllText($"{WorkPath}\\{fileName}.{DevelopmentEnvironment}.json");
            _configuration = JObject.Parse(json); //new JObject(JObject.Parse(json));
            return this;
        }

        public T GetSection<T>(string sectionName = null)
        {
            JToken section = _configuration[sectionName ?? typeof(T).Name];
            T value = section != null ? section.ToObject<T>() : default;
            return value;
        }

        public IConfigurationBuilder Build()
        {
            return this;
        }

        public static T BuildCore<T>(string workPath = null, bool isDevelopmentEnvironment = true)
        {
            var environmentName = isDevelopmentEnvironment ? "development" : "production";

            //IConfigurationBuilder builder = new ConfigurationBuilder()
            //   .SetBasePath(workPath ?? WorkPath)
            //   .AddJsonFile($"{FileName}.json", optional: true, reloadOnChange: true)
            //   .AddJsonFile($"{FileName}.{environmentName}.json", optional: true, reloadOnChange: true)
            //.AddEnvironmentVariables();

            //AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            //IConfigurationRoot Configuration = builder.Build();
            //AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

            //IConfigurationSection section = Configuration.GetSection(typeof(T).Name);
            //T appSettings = JsonConvert.DeserializeObject<T>(section.Value); //..Get<T>();

            return default;
        }
    }
}
