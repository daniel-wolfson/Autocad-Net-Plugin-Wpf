using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Internal
{
    public static class Configs
    {
        public delegate void ConfigSubmitEventHandler(object sender, EventArgs args);
        public delegate void ConfigLoadEventHandler(string sender);

        public static bool IsLoaded = false;
        public static event ConfigSubmitEventHandler ConfigSubmitEvent;
        public static event ConfigLoadEventHandler ConfigLoadEvent;

        /// <summary> Distance between members of chains (mm) </summary>
        public static float _chainDistance = 2;
        public static float ChainDistance
        {
            get { return _chainDistance; }
            set { _chainDistance = value; }
        }

        public static Dictionary<string, object> OptionsDictionary;
        //public static RuleCollection Rules = new RuleCollection();
        public static List<ITask> Tasks = new List<ITask>(); //TaskCollection Tasks = new TaskCollection();

        public static readonly Configuration CurrentAppConfig;
        public static ConnectionStringSettingsCollection ConnectionStrings;

        static Configs()
        {
            //HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data")
            //var str = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + @"\Desktop";
            //config = WebConfigurationManager.OpenWebConfiguration("/"); 
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();

            try
            {
                if (pluginSettings.UserConfigFileName != "")
                {
                    var configFileMap = new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = string.Concat(pluginSettings.UserSettingsPath, "User.config")
                    };
                    // Get the roaming configuration that applies to the current user.
                    CurrentAppConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                    //if (!AppSettingsContains("Doc")) AppSettingsCreate("Doc");
                    //var sections = config.Sections;
                    //ConfigurationSectionGroup csg = config.GetSectionGroup("applicationSettings");
                    //ConfigurationSectionCollection csc = csg.Sections;
                    //ConfigurationSection cs = csc.Get("Search.Properties.Settings");
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Plugin.Logger.Error($"{nameof(Configs)}.{nameof(Configs)}", ex);
            }
        }

        #region "AppSettings"

        // Actions:
        // * Create the AppSettings section.
        // * The get functions for use as GetSection(string)method 
        // * Access the configuration section. 
        // * Additions a new elements to the section collection.
        // * Read the connectionStrings

        public static bool AppSettingsContains(string stringKey)
        {
            return CurrentAppConfig.AppSettings.Settings.AllKeys.Contains(stringKey);
        }

        public static void AppSettingSetKey(string key, string value)
        {
            CurrentAppConfig.AppSettings.Settings[key].Value = value;
            CurrentAppConfig.AppSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
        }
        public static string AppSettingGetKey(string key)
        {
            return CurrentAppConfig.AppSettings.Settings[key].Value;
        }

        public static void AppSettingsCreate(string newKey)
        {
            //var newKey = "NewKey" + Convert.ToString(appStgCnt);
            var newValue = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

            if (!AppSettingsContains(newKey))
            {
                CurrentAppConfig.AppSettings.Settings.Add(newKey, newValue);

                // Save the configuration file.
                CurrentAppConfig.Save(ConfigurationSaveMode.Modified);

                // Force a reload of the changed section. This makes the new values available for reading.
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        //Returns an XML node object that represents the associated configuration-section object.
        public static string GetXmlDocument(string sectionName)
        {
            var appSettingSection = (AppSettingsSection)CurrentAppConfig.GetSection(sectionName);
            return appSettingSection.SectionInformation.GetRawXml();
        }

        //public static KeyValueConfigurationCollection GetSectionSettings(string sectionName)
        //{
        //    var section = (AppSettingsSection)Config.GetSection(sectionName);
        //    if (section != null)
        //    {
        //        return section.Settings[sectionName].Value;
        //    }
        //}

        public static Configuration Find(string fileName)
        {
            try
            {
                // no need to null check, ConfigurationManager.OpenMappedExeConfiguration will always return an object or throw ArgumentException
                return ConfigurationManager.OpenMappedExeConfiguration
                    (new ExeConfigurationFileMap { ExeConfigFilename = fileName }, ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(Configs)}.{nameof(Find)}", ex);
                return null;
            }
        }

        public static bool HasFile(string fileName)
        {
            var appConfiguration = ConfigurationManager.OpenMappedExeConfiguration
                (new ExeConfigurationFileMap { ExeConfigFilename = fileName }, ConfigurationUserLevel.None);
            // no need to null check, ConfigurationManager.OpenMappedExeConfiguration will always return an object or throw ArgumentException
            return appConfiguration.HasFile;
        }

        //Get object connection with properties such as: .Name, .ProviderName, .ConnectionString, ...;
        public static ConnectionStringSettings GetConnectionSettings(string connString)
        {
            return !string.IsNullOrEmpty(connString) ? ConfigurationManager.ConnectionStrings[connString] : null;
        }
        ////Get object connection with parameters such as string
        public static string GetConnectionString(string connString)
        {
            return !string.IsNullOrEmpty(connString) ? ConfigurationManager.ConnectionStrings[connString].ConnectionString : null;
        }

        #endregion

        // ???
        public static void Submit(object sender, EventArgs args)
        {
            var handler = ConfigSubmitEvent;
            if (handler != null) handler(sender, args);
        }

        //rising event the load configuration
        public static void Load(string configSetName)
        {
            var handler = ConfigLoadEvent;
            if (handler != null) handler(configSetName);
        }
    }
}