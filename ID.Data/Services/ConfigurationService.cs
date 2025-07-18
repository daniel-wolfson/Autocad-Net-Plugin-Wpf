using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel.Configuration;

namespace Intellidesk.AcadNet.Data.Services
{
    public sealed class ConfigurationService<T> where T : class
    {
        private static string _connectionString;

        #region Properties

        public static string ConnectionString
        {
            get { return _connectionString ?? (_connectionString = GetConnectionString()); }
            set { _connectionString = value; }
        }

        #endregion

        private static string GetConnectionString()
        {
            return _connectionString ?? (_connectionString = 
                GetConfiguration().ConnectionStrings.ConnectionStrings[typeof(T).Name].ConnectionString);
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

    public static class ConfigurationExtensions
    {
        public static ServiceModelSectionGroup GetSectionGroup(this Configuration config)
        {
            return ServiceModelSectionGroup.GetSectionGroup(config);
        }
    }
}
