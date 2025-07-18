using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel.Configuration;

namespace Intellidesk.Data.Services
{
    public sealed class ConfigurationManager<T> where T : class
    {
        private static string _connectionString;
        public static string ConnectionString
        {
            get { return _connectionString ?? (_connectionString = GetConnectionString()); }
            set { _connectionString = value; }
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return GetConfiguration().ConnectionStrings.ConnectionStrings; }
        }

        public static string GetConnectionString()
        {
            ConnectionStringSettings connection;
            try
            {
                connection = GetConfiguration().ConnectionStrings.ConnectionStrings[typeof(T).Name];
                //if (!Database.Exists(connection.ConnectionString))
                //    connection = GetConfiguration().ConnectionStrings.ConnectionStrings[typeof(T).Name + "Local"];
            }
            catch (Exception)
            {
                connection = GetConfiguration().ConnectionStrings.ConnectionStrings[typeof(T).Name + "Local"];
            }

            return connection != null
                ? connection.ConnectionString
                : "Name=" + typeof(T).Name;
        }

        public static Configuration GetConfiguration()
        {
            var localPath = new Uri(typeof(T).Assembly.CodeBase).LocalPath;
            var config = ConfigurationManager.OpenExeConfiguration(localPath); //OpenExeConfiguration(ConfigurationUserLevel.None)
            return config;
        }

        public static ServiceModelSectionGroup GetSectionGroup(Configuration config)
        {
            return ServiceModelSectionGroup.GetSectionGroup(config);
        }

        public static AppSettingsSection GetAppSettings()
        {
            UriBuilder uri = new UriBuilder(typeof(T).Assembly.CodeBase);
            Configuration config = ConfigurationManager.OpenExeConfiguration(uri.Path);
            return (AppSettingsSection)config.GetSection("appSettings");
        }

        public static TObject Setting<TObject>(string name) where TObject : class
        {
            var nfi = new NumberFormatInfo()
            {
                NumberGroupSeparator = "",
                CurrencyDecimalSeparator = "."
            };
            return (TObject)Convert.ChangeType(GetAppSettings().Settings[name].Value, typeof(TObject), nfi);
        }
    }
}
