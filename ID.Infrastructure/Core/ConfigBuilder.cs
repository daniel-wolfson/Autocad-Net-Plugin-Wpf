using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace General.Infrastructure.Core
{
    public static class ConfigBuilder
    {
        private static string workPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
        public static string WorkPath { get { return workPath; } }

        public static string FileName { get; set; } = "appsettings";

        public static T Build<T>(string workPath = null, bool isDevelopmentEnvironment = true)
        {
            var EnvironmentName = isDevelopmentEnvironment ? "development" : "production";

            var json = File.ReadAllText($"{WorkPath}\\{FileName}.json");
            JToken appSettings = JObject.Parse(json)[typeof(T).Name];

            T value = appSettings.ToObject<T>();
            //T obj = JsonSerializer.Deserialize<T>(appSettings.Value);

            //IConfigurationBuilder builder = new ConfigurationBuilder()
            //   .SetBasePath(WorkPath)
            //   .AddJsonFile($"{FileName}.json", optional: true, reloadOnChange: true)
            //   .AddJsonFile($"{FileName}.{EnvironmentName}.json", optional: true, reloadOnChange: true);
            //.AddEnvironmentVariables();

            //AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            //IConfigurationRoot Configuration = builder.Build();
            //AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

            //var section = Configuration.GetSection(typeof(T).Name);
            //T appSettings = section.Get<T>();

            return value; //section.Get<T>();
        }
    }
}
