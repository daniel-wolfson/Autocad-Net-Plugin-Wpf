using ID.Infrastructure.Commands;
using ID.Infrastructure.Interfaces;
using Intellidesk.Data.General;
using Intellidesk.Resources.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;


namespace ID.Infrastructure.Models
{
    [DataContract]
    [KnownType(typeof(AppSettings<PluginSettings>))]
    public class PluginSettings : AppSettings<PluginSettings>, IPluginSettings
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public bool Busy { get; set; }

        [DataMember(Order = 4)]
        public string Copyright { get; set; }

        [DataMember(Order = 5)]
        public string CurrentFolder { get; set; }

        [DataMember(Order = 6)]
        public string MapitWebHost { get; set; }

        [DataMember(Order = 7)]
        public string TemplateFullPath { get; set; }

        [DataMember(Order = 8)]
        public string Version { get; set; }

        [DataMember(Order = 9)]
        public bool IsRegAppTable { get; set; }

        [DataMember(Order = 10)]
        public bool IsDemo { get; set; }

        [DataMember(Order = 11)]
        public bool IsComposed { get; set; }

        [DataMember(Order = 12)]
        public bool ShowAllFolders { get; set; }

        [DataMember(Order = 13)]
        public bool CanFullSearch { get; set; }

        [DataMember(Order = 14)]
        public string DbHost { get; set; } = Settings.Default.DbHost;

        [DataMember(Order = 15)]
        public string UserDomainName { get; set; }

        [DataMember(Order = 16)]
        public string Prompt { get; set; }

        [DataMember(Order = 17)]
        public string ResourceImages { get; set; }

        [DataMember(Order = 18)]
        public string ResourceClass { get; set; }

        /// <summary> root path of placement of assembly </summary>
        [DataMember(Order = 19)]
        public string RootPath { get; set; }

        /// <summary> resource path of placement of assembly </summary>
        [DataMember(Order = 20)]
        public string ResourcePath { get; set; }

        /// <summary> resource lib of blocks </summary>
        [DataMember(Order = 21)]
        public string ResourceLib { get; set; }

        [DataMember(Order = 22)]
        public int ReportIndex { get; set; }

        [DataMember(Order = 23)]
        public bool SearchIncludeSubdir { get; set; }

        private int _saveAsAutoSaveTime = 10;
        [DataMember(Order = 24)]
        public int SaveAsAutoSaveTime
        {
            get { return _saveAsAutoSaveTime; }
            set { _saveAsAutoSaveTime = value; }
        }

        /// <summary> path to user xml settings </summary>
        [DataMember(Order = 25)]
        public string UserSettingsPath { get; set; }

        /// <summary> User.config </summary>
        [DataMember(Order = 26)]
        public string UserConfigFileName { get; set; }

        [DataMember(Order = 27)]
        public string UserSettingsFileName { get; set; }

        [DataMember(Order = 28)]
        public string LayoutFiltersFileName { get; set; }

        //[ScriptIgnore]
        public string TempPath { get; set; }

        [DataMember(Order = 29)]
        public List<string> ScaleFactors { get; set; }

        [DataMember(Order = 30)]
        public string PaletteSetId { get; set; } = Settings.Default.PaletteSetId;

        [DataMember(Order = 31)]
        public int ProjectExplorerFoldersSectionHeight { get; set; }

        [DataMember(Order = 32)]
        public int ProjectExplorerFilesSectionHeight { get; set; }
        [DataMember(Order = 33)]
        public int ProjectExplorerPropertySectionHeight { get; set; }
        [DataMember(Order = 34)]
        public bool ProjectExplorerPropertySectionDisplay { get; set; }
        [DataMember(Order = 35)]
        public string ToolPanelTop { get; set; }
        [DataMember(Order = 36)]
        public int ToolPanelWidth { get; set; }
        [DataMember(Order = 37)]
        public int ToolPanelLastWidth { get; set; }
        [DataMember(Order = 38)]
        public int TabIndex { get; set; }
        [DataMember(Order = 39)]
        public string AppHost { get; set; }
        [DataMember(Order = 40)]
        public string IntelliDeskApiUrlSample { get; set; }
        private int _zoomDisplayFactor;
        [DataMember(Order = 41)]
        public int ZoomDisplayFactor
        {
            get { return _zoomDisplayFactor; }
            set
            {
                _zoomDisplayFactor = value;
                //OnPropertyChanged(() => ZoomDisplayFactor);
                RaisePropertyChanged("ZoomDisplayFactor");
            }
        }

        [DataMember(Order = 42)]
        public string GisLayerName { get; set; }
        [DataMember(Order = 43)]
        public string MapitApiHost { get; set; } = Settings.Default.MapInfoApiHost;

        private string _mapInfoApiPointUrl = Settings.Default.MapInfoApiPointUrlFormat;
        [DataMember(Order = 44)]
        public string MapitApiPointUrl
        {
            get { return _mapInfoApiPointUrl; }
            set { _mapInfoApiPointUrl = string.IsNullOrEmpty(value) ? _mapInfoApiPointUrl : value; }
        }

        private string _mapInfoApiFindUrl = Settings.Default.MapinfoApiFindUrlFormat;
        [DataMember(Order = 45)]
        public string MapitApiFindUrl
        {
            get { return _mapInfoApiFindUrl; }
            set { _mapInfoApiFindUrl = string.IsNullOrEmpty(value) ? _mapInfoApiFindUrl : value; }
        }

        private string _mapItPath = Settings.Default.MapItPath;
        [DataMember(Order = 46)]
        public string MapItPath
        {
            get { return _mapItPath; }
            set { _mapItPath = string.IsNullOrEmpty(value) ? _mapItPath : value; }
        }
        [DataMember(Order = 47)]
        public string BlockFilePath { get; set; }

        [DataMember(Order = 48)]
        public string BlockBasePoint { get; set; }

        [DataMember(Order = 49)]
        public List<string> IncludeFolders { get; set; } = new List<string>();

        [DataMember(Order = 50)]
        public List<IWorkItem> WorkItems { get; set; } = new List<IWorkItem>();

        [DataMember(Order = 51)]
        public string AppServiceSigningKey { get; set; }

        [DataMember(Order = 52)]
        public double AppUserTokenExpires { get; set; }

        [DataMember(Order = 52)]
        public string MapitApiEndPoint { get; set; }

        [DataMember(Order = 53)]
        public string AppUserToken { get; set; }

        public static PluginSettings Default
        {
            get
            {
                var assembly = typeof(PluginSettings).Assembly;
                var productName = FileVersionInfo.GetVersionInfo(assembly.Location).ProductName;
                var resourcePath = Path.GetDirectoryName(assembly.Location) + "\\".Replace("Win64\\", "Resources\\");

                var tempPath = Path.GetTempPath() + productName + "\\";
                bool exists = Directory.Exists(tempPath);
                if (!exists)
                    Directory.CreateDirectory(tempPath);

                return new PluginSettings()
                {
                    GisLayerName = Settings.Default.WorkLayerName,
                    AppHost = Settings.Default.IntelliDeskHost,
                    MapitWebHost = Settings.Default.MapitWebHost,
                    IntelliDeskApiUrlSample = Settings.Default.IntelliDeskApiUrlSample,
                    ResourceImages = Settings.Default.ResourceImages,
                    ResourceClass = Settings.Default.ResourceClass,
                    ToolPanelTop = Settings.Default.ToolPanelTop,
                    ToolPanelWidth = Settings.Default.ToolPanelWidth,
                    ToolPanelLastWidth = Settings.Default.ToolPanelWidth,
                    ZoomDisplayFactor = Settings.Default.ZoomDisplayFactor,
                    ProjectExplorerFilesSectionHeight = Settings.Default.ProjectExplorerFilesSectionHeight,

                    MapItPath = Path.Combine("http:", Settings.Default.MapitWebHost, "mapit/mapit.html"),
                    LayoutFiltersFileName = "AcadNetLayoutFilters.xml",
                    Name = string.IsNullOrEmpty(Settings.Default.ProjectName) ? assembly.GetName().Name : Settings.Default.ProjectName,
                    Prompt = "\n" + CommandNames.UserGroup + ":" + char.ConvertFromUtf32(160),
                    ResourcePath = resourcePath,
                    ResourceLib = resourcePath + "lib.dwg",
                    ScaleFactors = new List<string> { "1", "2", "3", "5", "7", "10" },
                    TempPath = Path.GetTempPath() + productName + "\\",
                    UserSettingsPath = assembly.Location,
                    UserDomainName = Environment.UserDomainName,
                    Version = "(v" + assembly.GetName().Version + ")"

                };
            }
        }
    }
}