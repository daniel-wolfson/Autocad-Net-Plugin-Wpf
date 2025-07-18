using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using System.Data.Entity;

using NetTools;
using AcadNetTools;
using AcadNet.Data.Models;
using AcadNet.Data.Repository;

namespace AcadNet.Data.Models
{
    public interface IRuleRepository
    {
        IEnumerable<IRule> GetRules(string configSetName);
    }

    public interface IConfigRepository
    {
        IEnumerable<Config> GetConfigs();
        IEnumerable<Config> GetConfigsDistinct();
        //IEnumerable<Tuple<string, string[]>> GetLayoutCatalogOptions();
        //IEnumerable<Tuple<string, string[]>> GetLayoutCatalogSites();
        //IEnumerable<string> GetToolNameAttributes();
        //IEnumerable<Config> GetFrameTypes();
        //IEnumerable<string> GetLayerTypesPatterns(int parameter);
        void SetConfigFilterFor(Func<Config, bool> exp);
    }

    public interface IUserSettingRepository
    {
        IEnumerable<BaseUserSetting> GetUserSettings();
        void SaveSettings(IEnumerable<BaseUserSetting> userSettings);
    }
}
