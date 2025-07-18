using System;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface IConfigService
    {
        event ConfigSubmitEventHandler ConfigSubmitEvent;
        event ConfigLoadEventHandler ConfigLoadEvent;
        float ChainDistance { get; set; }
        bool AppSettingsContains(string stringKey);
        void AppSettingSetKey(string key, string value);
        string AppSettingGetKey(string key);
        void AppSettingsCreate(string newKey);
        string GetXmlDocument(string sectionName);
        void Submit(object sender, EventArgs args);
        void Load(string configSetName);
    }
}