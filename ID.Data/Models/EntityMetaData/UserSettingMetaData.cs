using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.EntityMetaData
{

    public enum ProjectStatus
    {
        Continue,
        Debug,
        Locked,
        New
    }
    /// <summary> Application settings </summary>
    [Serializable(), DefaultProperty("Id")]
    public class UserSettingMetaData : BaseEntity
    {
        public int UserSettingId { get; set; }

        [Description("ConfigSet name"), Category("Identity")]
        public string ConfigSetName { get; set; }

        [Description("Distance between basic drawing elements"), Category("Identity")]
        public double ChainDistance { get; set; }

        [Description("Color Brush"), Category("Identity"), DefaultValue(191)]
        public int ColorIndex { get; set; }

        [Description("The day this work was started"), Category("Identity")]
        public DateTime DateStarted { get; set; }

        [Description("Drive"), Category("Identity")]
        public string Drive { get; set; }

        [Description("Is Active"), Category("Identity")]
        public bool IsActive { get; set; }

        [Description("Is Drawing color mode"), Category("Identity")]
        public bool IsColorMode { get; set; }

        [Description("Layout Explorer Row Splitter Position"), Category("Identity")]
        public short ProjectExplorerRowSplitterPosition { get; set; }

        [Description("Layout Explorer Column Splitter Position"), Category("Identity")]
        public short ProjectExplorerPGridColumnSplitterPosition { get; set; }

        [Description("Percent"), Category("Identity")]
        public short Percent { get; set; }

        [Description("Project status"), Category("Identity")]
        public ProjectStatus ProjectStatus { get; set; }

        [Description("Toggle Layout DataTemplate Selector"), Category("Identity")]
        public short ToggleLayoutDataTemplateSelector { get; set; }

        [Description("The user's name"), Category("Identity")]
        public string User { get; set; }

        [Description("MinWidth"), Category("Identity")]
        public int MinWidth { get; set; }

        /// <summary> Save will be called on a specific instance </summary>
        public void Save(string path)
        {
            try
            {
                var xs = new XmlSerializer(typeof(UserSetting));
                var sw = new StreamWriter(path, false); //ProjectManager.RootPath + ProjectManager.UserSettingsFileName
                xs.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception)
            {
                //var ed = Application.DocumentManager.MdiActiveDocument.Editor; //!!!!!!!!
                //ed.WriteMessage("\nUnable to save the application settings: {0}", ex);
            }
        }
    }
}