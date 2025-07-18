using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.Data.Models.Cad;
using System;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IBaseViewModel
    {
        IPluginSettings PluginSettings { get; }
        UserSetting CurrentUserSetting { get; set; }
        bool IsActive { get; set; }
        bool IsCanceled { get; set; }
        bool IsLoaded { get; set; }
        bool IsReadOnly { get; set; }
        IPanelViewModel Parent { get; set; }
        bool SwitchSizeMode { get; set; }

        bool IsDwgCompatible(Type[] typeFilterOn = null, string[] attributePatternOn = null);
        bool IsDwgOpen(Type[] typeFilterOn = null, string[] attributePatternOn = null);
    }
}