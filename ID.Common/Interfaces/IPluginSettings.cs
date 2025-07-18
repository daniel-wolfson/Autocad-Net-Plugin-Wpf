using System.Collections.Generic;

namespace Intellidesk.Interfaces
{
    public interface IPluginSettings
    {
        string AppHost { get; set; }
        bool Busy { get; set; }
        string BlockFilePath { get; set; }
        string BlockBasePoint { get; set; }

        string CurrentFolder { get; set; }
        string Copyright { get; set; }
        bool CanFullSearch { get; set; }
        string DbHost { get; set; }

        string GisLayerName { get; set; }
        List<string> IncludeFolders { get; set; }
        string IntelliDeskApiUrlSample { get; set; }
        bool IsComposed { get; set; }
        bool IsDemo { get; set; }
        bool IsRegAppTable { get; set; }

        string LayoutFiltersFileName { get; set; }
        string MapitWebHost { get; set; }
        string MapitApiHost { get; set; }
        string MapitApiEndPoint { get; set; }
        string MapitApiPointUrl { get; set; }
        string MapitApiFindUrl { get; set; }
        string MapItPath { get; set; }
        string Name { get; set; }

        string PaletteSetId { get; set; }
        string Prompt { get; set; }
        int ProjectExplorerFilesSectionHeight { get; set; }
        int ProjectExplorerFoldersSectionHeight { get; set; }
        bool ProjectExplorerPropertySectionDisplay { get; set; }
        int ProjectExplorerPropertySectionHeight { get; set; }

        string ResourceClass { get; set; }
        string ResourceImages { get; set; }
        string ResourceLib { get; set; }
        string ResourcePath { get; set; }
        string RootPath { get; set; }
        int ReportIndex { get; set; }

        int SaveAsAutoSaveTime { get; set; }
        List<string> ScaleFactors { get; set; }
        bool ShowAllFolders { get; set; }
        void Save();
        bool SearchIncludeSubdir { get; set; }

        string TemplateFullPath { get; set; }
        string ToolPanelTop { get; set; }
        int ToolPanelWidth { get; set; }
        int ToolPanelLastWidth { get; set; }
        int TabIndex { get; set; }
        string TempPath { get; set; }

        string UserId { get; set; }
        string UserDomainName { get; set; }
        string UserConfigFileName { get; set; }
        string UserSettingsFileName { get; set; }
        string UserSettingsPath { get; set; }

        string Version { get; set; }
        int ZoomDisplayFactor { get; set; }

        List<IWorkItem> WorkItems { get; set; }

        string AppServiceSigningKey { get; set; }

        double AppUserTokenExpires { get; set; }

        string AppUserToken { get; set; }

    }
}