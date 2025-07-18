using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Intellidesk.Data.Models.Cad
{
    [MetadataType(typeof(UserSettingMetaData))]
    public partial class UserSetting : BaseEntity
    {
        public int UserSettingId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string ConfigSetName { get; set; }
        public float ChainDistance { get; set; }
        public System.DateTime DateStarted { get; set; }
        public bool IsActive { get; set; }
        public bool IsColorMode { get; set; }
        public short ProjectExplorerRowSplitterPosition { get; set; }
        public short ProjectExplorerPGridColumnSplitterPosition { get; set; }
        public short Percent { get; set; }
        public string ProjectStatus { get; set; }
        public short ToggleLayoutDataTemplateSelector { get; set; }
        public int MinWidth { get; set; }
        public Nullable<decimal> LayoutId { get; set; }
        public string Drive { get; set; }
        public int ColorIndex { get; set; }
        public double Pos_X { get; set; }
        public double Pos_Y { get; set; }
        public double Pos_Z { get; set; }

        public System.Data.Entity.Spatial.DbGeography GeoPos { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public int UserSettingUser_UserSetting_UserId { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public int UserSettingUser_UserSetting_UserSettingId { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public decimal Layout_LayoutID { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public virtual ILayout Layout { get; set; }

        [XmlIgnore]
        [ScriptIgnore]

        public virtual User User { get; set; }
    }

    public partial class UserSetting
    {
        public static IPluginSettings PluginSettings
        {
            get { return Plugin.GetService<IPluginSettings>(); }
        }

        public static UserSetting Load(string filename = "")
        {
            UserSetting ret = null;

            if (filename != "")
                PluginSettings.UserSettingsFileName = filename;

            try
            {
                var xs = new XmlSerializer(typeof(UserSetting));
                var sr = new StreamReader(PluginSettings.RootPath + PluginSettings.UserSettingsFileName);
                ret = (UserSetting)xs.Deserialize(sr);
                sr.Close();
            }
            catch
            {
                var userSetting = new UserSetting();
                try
                {
                    var xs = new XmlSerializer(typeof(UserSetting));
                    var sw = new StreamWriter(PluginSettings.RootPath + PluginSettings.UserSettingsFileName, false);
                    xs.Serialize(sw, userSetting);
                    sw.Close();
                }
                catch
                {
                }
                return userSetting;
            }
            return ret;
        }

        /// <summary> Save will be called on a specific instance </summary>
        public void Save(string path)
        {
            try
            {
                var xs = new XmlSerializer(typeof(UserSettingMetaData));
                var sw = new StreamWriter(path, false); //ProjectManager.RootPath + ProjectManager.UserSettingsFileName
                xs.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception)
            {
                //var ed = Application.DocumentManager.MdiActiveDocument.Editor; !!!!!!!!!!!
                //ed.WriteMessage("\nUnable to save the application settings: {0}", ex);
            }
        }
    }
}
