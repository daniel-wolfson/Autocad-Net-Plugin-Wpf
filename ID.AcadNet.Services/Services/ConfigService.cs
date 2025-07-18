using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.Infrastructure;

namespace Intellidesk.AcadNet.Services
{
    public class TaskCollection1 : CollectionBase
    {
        #region "Methods"

        public int Add(ITask value)
        {
            return (List.Add(value));
        }

        public void AddRange(ITask[] rules)
        {
            rules.ToList().ForEach(r => Add(r));
        }

        public int IndexOf(ITask value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, ITask value)
        {
            List.Insert(index, value);
        }

        public void Remove(ITask value)
        {
            List.Remove(value);
        }

        public bool Contains(ITask value)
        {
            // If value is not of type Int16, this will return false.
            return (List.Contains(value));
        }

        protected override void OnInsert(int index, Object value)
        {
            // Insert additional code to be run only when inserting values.
        }

        protected override void OnRemove(int index, Object value)
        {
            // Insert additional code to be run only when removing values.
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            // Insert additional code to be run only when setting values.
        }

        protected override void OnValidate(object value)
        {
            if (value.GetType().GetInterface("ITask", true) != typeof(ITask))
                throw new ArgumentException("value must be of type ITask.", "value");
        }

        public new IEnumerator<ITask> GetEnumerator()
        {
            for (var i = 0; i < List.Count; i++)
            {
                yield return (ITask)List[i];
            }
        }

        #endregion "Methods"
    }

    public class RuleCollection : CollectionBase //, IEnumerable<IRule>
    {
        #region "Property"

        [DefaultValue(null)]
        public static Point3d MainPosition { get; set; }

        [DefaultValue(false)]
        public static bool IncludeNested { get; set; }

        public List<Tuple<string, int?, string>> ConfigDictionary = new List<Tuple<string, int?, string>>();

        public long StartBlockIndex { get; set; }

        public long LayoutId { get; set; }

        public Type[] TypeFilterOn { get; set; }

        public string[] AttributePatternOn { get; set; }

        public string[] LayerPatternOn { get; set; }

        public List<LayerItem> LayerItemsFilterOn = new List<LayerItem>();

        public string[] LayoutCatalogSite { get; set; }

        public string[] LayoutCatalogOptions { get; set; }

        public string[] TooNameAttributes { get; set; }

        public IRule this[int index]
        {
            get { return ((IRule)List[index]); }
            set { List[index] = value; }
        }

        #endregion

        #region "Methods"

        public int Add(IRule value)
        {
            //var layerItem = new LayerItem { Color = Color.FromColorIndex(ColorMethod.ByAci, value.ColorIndex), Name = value.LayerPlace };
            //if (!LayerManager.ListItems.Contains(layerItem)) LayerManager.ListItems.Add(layerItem);
            return (List.Add(value));
        }

        public void AddRange(IRule[] rules)
        {
            rules.ToList().ForEach(r => Add(r));
        }

        public IRule GetRule(string layerPattern)
        {
            return List.Cast<IRule>().ToList()
                       .Find(r => (r.LayerPatternOn.Contains(layerPattern) && !r.LayerPatternOff.Contains(layerPattern)));
        }

        public int IndexOf(IRule value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, IRule value)
        {
            List.Insert(index, value);
        }

        public void Remove(IRule value)
        {
            List.Remove(value);
        }

        public bool Contains(IRule value)
        {
            // If value is not of type Int16, this will return false.
            return (List.Contains(value));
        }

        protected override void OnInsert(int index, Object value)
        {
            // Insert additional code to be run only when inserting values.
        }

        protected override void OnRemove(int index, Object value)
        {
            // Insert additional code to be run only when removing values.
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            // Insert additional code to be run only when setting values.
        }

        protected override void OnValidate(object value)
        {
            if (value.GetType().GetInterface("IRule", true) != typeof(IRule))
                throw new ArgumentException("value must be of type Int16.", "value");
        }

        public new IEnumerator<IRule> GetEnumerator()
        {
            for (var i = 0; i < List.Count; i++)
            {
                yield return (IRule)List[i];
            }
        }

        #endregion "Methods"

        #region "Ext.method"

        public Type[] XGetFilterTypesOn()
        {
            var filterTypes = new List<Type>();
            List.Cast<IRule>().ToList().ForEach(r => filterTypes.AddRange(r.TypeFilterOn));
            return filterTypes.ToArray();
        }

        public string[] XGetFilterAttributesOn()
        {
            var filterTypes = new List<string>();
            List.Cast<IRule>().ToList().ForEach(r => filterTypes.AddRange(r.AttributePatternOn));
            return filterTypes.ToArray();
        }

        public Type[] XGetSingleFilterTypesOn()
        {
            return TypeFilterOn;
        }

        public string[] XGetSingleFilterBlockAttributesOn()
        {
            return AttributePatternOn;
        }

        public List<LayerItem> XGetLayerItemsDistinct()
        {
            return List.Cast<IRule>().ToList()
                       .Select(r => new LayerItem 
                            { Color = Color.FromColorIndex(ColorMethod.ByAci, (short)r.ColorIndex), Name = r.LayerDestination })
                       .Distinct()
                       .ToList();
        }

        #endregion
    }

    public delegate void ConfigSubmitEventHandler(object sender, EventArgs args);
    public delegate void ConfigLoadEventHandler(string sender);

    public static class Config
    {
        public static bool IsLoaded = false;
        public static event ConfigSubmitEventHandler ConfigSubmitEvent;
        public static event ConfigLoadEventHandler ConfigLoadEvent;
        
        /// <summary> Distance between members of chains (mm) </summary>
        public static float _chainDistance = 2;
        public static float ChainDistance {
            get { return _chainDistance; }
            set { _chainDistance = value; }
        }

        public static Dictionary<string, object> OptionsDictionary;
        //public static RuleCollection Rules = new RuleCollection();
        public static List<ITask> Tasks = new List<ITask>(); //TaskCollection Tasks = new TaskCollection();

        public static readonly Configuration CurrentUserConfig;
        public static ConnectionStringSettingsCollection ConnectionStrings;

        static Config()
        {
            //HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data")
            //var str = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + @"\Desktop";
            //config = WebConfigurationManager.OpenWebConfiguration("/"); 
            try
            {
                if (Plugin.UserConfigFileName != "")
                {
                    var configFileMap = new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = String.Concat(Plugin.UserSettingsPath, "User.config")
                    };
                    // Get the roaming configuration that applies to the current user.
                    CurrentUserConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                    if (!AppSettingsContains("Doc")) AppSettingsCreate("Doc");
                    //var sections = config.Sections;
                    //ConfigurationSectionGroup csg = config.GetSectionGroup("applicationSettings");
                    //ConfigurationSectionCollection csc = csg.Sections;
                    //ConfigurationSection cs = csc.Get("Search.Properties.Settings");
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Add(ex); //Log.Add("Error", "Exception error: {0}", e.ToString());
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
            return CurrentUserConfig.AppSettings.Settings.AllKeys.Contains(stringKey);
        }

        public static void AppSettingSetKey(string key, string value)
        {
            CurrentUserConfig.AppSettings.Settings[key].Value = value;
            CurrentUserConfig.AppSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
        }
        public static string AppSettingGetKey(string key)
        {
            return CurrentUserConfig.AppSettings.Settings[key].Value;
        }

        public static void AppSettingsCreate(string newKey)
        {
            //var newKey = "NewKey" + Convert.ToString(appStgCnt);
            var newValue = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

            if (!AppSettingsContains(newKey))
            {
                CurrentUserConfig.AppSettings.Settings.Add(newKey, newValue);

                // Save the configuration file.
                CurrentUserConfig.Save(ConfigurationSaveMode.Modified);

                // Force a reload of the changed section. This makes the new values available for reading.
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        //Returns an XML node object that represents the associated configuration-section object.
        public static string GetXmlDocument(string sectionName)
        {
            var appSettingSection = (AppSettingsSection)CurrentUserConfig.GetSection(sectionName);
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
                Log.Add(ex);
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
            return !String.IsNullOrEmpty(connString) ? ConfigurationManager.ConnectionStrings[connString] : null;
        }
        ////Get object connection with parameters such as string
        public static string GetConnectionString(string connString)
        {
            return !String.IsNullOrEmpty(connString) ? ConfigurationManager.ConnectionStrings[connString].ConnectionString : null;
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
