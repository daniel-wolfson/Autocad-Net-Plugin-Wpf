using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

using NetTools;
using AcadNetTools;
using AcadNet.Data.Models;
using AcadNet.Data.Models.Intel;
using AcadNet.Data.Repository;
using Layout = AcadNet.Data.Models.Intel.Layout;

namespace AcadNet.Data.Models.Intel
{
    public interface ILayoutRepository : IRepository<Layout>
    {
        void SetLayouts(IEnumerable<Layout> layouts);
    }

    public interface ILayoutFilterRepository
    {
        IEnumerable<LayoutFilter> GetLayoutFilters();
        void SaveSettings(IEnumerable<LayoutFilter> layoutFilters);
    }

    /// <summary> Simulates a Layout data source, which would normally come from a database </summary>
    [Export(typeof(ILayoutRepository))]
    public class LayoutRepository : BaseRepository<Layout>, ILayoutRepository
    {
        private static readonly string _filename = @"Models\LayoutSettings.xml";

        public LayoutRepository(Lsds2Context context) : base(context) { }

        // Get Layouts Settings
        public IEnumerable<Layout> GetLayoutsSettings()
        {
            if (!File.Exists(_filename))
            {
                throw new FileNotFoundException("DataSource file could not be found", _filename);
            }

            var layouts = System.Xml.Linq.XDocument.Load(_filename).Root.Elements("Layout")
                .Select(x => new Layout()
                {
                    LayoutName = x.Element("Title").ToString(),
                    CreatedBy = x.Element("CreatedBy").ToString(),
                    ModifiedBy = x.Element("ModifiedBy").ToString()
                });

            return layouts;
        }

        // Get Layouts
        public override IEnumerable<Layout> GetAll()
        {
            if (NetTools.ProjectManager.IsDemo)
                return (IEnumerable<Layout>)MakeDemoData();

            return _dbset.AsEnumerable<Layout>();
        }

        public IEnumerable<Layout> GetAll_old()
        {
            //if (ProjectManager.IsDemo) return MakeDemoData();

            var layouts = new List<Layout>();
            using (var context = DataToolsManager.CreateContext<Lsds2Context>())
            {
                //if (ProjectManager.IsDemo) layouts = (List<Layout>)MakeDemoData();

                if (context != null)
                {
                    try
                    {
                        context.Layouts.ToList().ForEach(x =>
                            layouts.Add(new Layout
                            {
                                AccessType = x.AccessType,
                                BuildingLevels = x.BuildingLevels,
                                CADFileName = x.CADFileName,
                                CreatedBy = x.CreatedBy,
                                Comment = x.Comment,
                                DateCreated = x.DateCreated,
                                DateModified = x.DateModified,
                                FSA = x.FSA ?? false,
                                LayoutID = x.LayoutID,
                                LayoutName = x.LayoutName,
                                LayoutType = x.LayoutType,
                                LayoutContents = x.LayoutContents,
                                LayoutVersion = x.LayoutVersion,
                                LayoutState = x.LayoutState,
                                ModifiedBy = x.ModifiedBy,
                                ProcessName1 = x.ProcessName1,
                                ProcessName2 = x.ProcessName2,
                                ProcessName3 = x.ProcessName3,
                                ProcessName4 = x.ProcessName4,
                                SiteName = x.SiteName,
                                WSPW1 = x.WSPW1,
                                WSPW2 = x.WSPW2,
                                WSPW3 = x.WSPW3,
                                WSPW4 = x.WSPW4,
                                Visible = x.Visible
                            }));
                    }
                    catch (Exception)
                    {
                        //layouts = MakeDemoData();
                    }
                }
            }
            return layouts;
        }

        // Saves layouts
        public void SetLayouts(IEnumerable<Layout> layouts)
        {
            if (layouts == null)
                throw new ArgumentNullException("layouts");

            new System.Xml.Linq.XDocument(layouts).Save(_filename);
        }

        public List<ILayout> MakeDemoData()
        {
            return new List<ILayout> { BaseLayout.MakeLayoutSample<Layout>() };
        }
    }

    /// <summary> Simulates a Config data source, which ... </summary>
    [Export(typeof(IConfigRepository))]
    public class ConfigRepository : IConfigRepository
    {
        private List<Config> _configs = new List<Config>();
        public Func<Config, bool> Exp = null;

        // Retrieves Configs
        public IEnumerable<Config> GetConfigs()
        {
            var grps = new List<Config>();
            using (var context = DataToolsManager.CreateContext<Lsds2Context>())
            {
                if (context != null)
                {
                    try
                    {
                        if (ProjectManager.IsDemo)
                        {
                            _configs = MakeDemoData();
                        }
                        else
                        {
                            _configs = context.LSDS_Config.Select(x =>
                            new Config { ConfigSetName = x.ConfigSetName, ParamName = x.ParameterName, Int1 = x.Int1, Str1 = x.Str1, Str2 = x.Str2, LongStr = x.LongStr }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Add(ex);
                        MakeDemoData();
                    }
                    finally
                    {
                        grps = _configs.GroupBy(x => x.ConfigSetName).Select(grp => grp.First()).ToList();
                        foreach (var group in grps)
                        {
                            var prms = _configs.Select(x => x.ParamName).Distinct().ToList();
                            foreach (var par in prms)
                            {
                                group.LayoutOptions.AddRange(GetLayoutDictionaries(group.ConfigSetName, par));
                            }
                        }
                    }
                }
            }
            return grps;
        }

        public IEnumerable<Config> GetConfigsDistinct()
        {
            return GetConfigs().GroupBy(x => x.ConfigSetName).Select(grp => grp.First());
        }

        public IEnumerable<string> GetConfigNames()
        {
            return _configs.Select(y => y.ConfigSetName).Distinct();
        }

        public IEnumerable<LayoutDictionary> GetLayoutDictionaries(string configSetName, string paramName)
        {
            switch (paramName)
            {
                case "FRAME_TYPE_ID":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParamName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1, Value = opt.Str1, ParameterName = paramName });
                case "LAYOUT_CATALOG_OPTIONS":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParamName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str2, ParameterName = paramName });
                case "LAYOUT_CATALOG_SITE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParamName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.LongStr, ParameterName = paramName });
                case "LAYER_TYPE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParamName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1, Value = opt.Str1, ParameterName = paramName });
                case "TOOL_NAME_ATTRIBUTE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParamName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str1, ParameterName = paramName });

            }
            return new List<LayoutDictionary>();
        }

        public void SetConfigFilterFor(Func<Config, bool> exp)
        {
            Exp = exp;
            _configs = exp != null ? _configs.Where(Exp).ToList() : _configs = GetConfigs().ToList();
        }

        public bool IsEmpty()
        {
            return _configs.Count > 0;
        }

        public List<Config> MakeDemoData()
        {
            _configs.Clear();
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "ACCESS_TYPE",
                Str2 = "typ1, typ2",
                LongStr = "typ1, typ2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYOUT_CATALOG_SITE",
                Int1 = 1,
                Str1 = "LC - Lachish Israel",
                Str2 = "site1, site2",
                LongStr = "site1, site2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "LAYOUT_CONTENT",
                Str2 = "content1, content2",
                LongStr = "content1, content2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "CONTENT_TYPE",
                Str2 = "type1, type2",
                LongStr = "type1, type2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "PROCESSES",
                Str2 = "proc1, proc2",
                LongStr = "proc1, proc2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "ENTITY_CODES",
                Str2 = "ENTITY_CODES",
                LongStr = "ENTITY_CODES"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "ENTITY_CODE",
                Str2 = "ENTITY_CODE",
                LongStr = "ENTITY_CODE"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "TOOL_ID",
                Str2 = "TOOL_ID",
                LongStr = "TOOL_ID"
            });

            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "FRAME_TYPE_ID",
                Int1 = 1,
                Str1 = "Fysical",
                Str2 = "Fysical",
                LongStr = "Fysical"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "FRAME_TYPE_ID",
                Int1 = 2,
                Str1 = "Clearence",
                Str2 = "Clearence",
                LongStr = "Clearence"
            });

            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYER_TYPE",
                Int1 = 1,
                Str1 = "*EQPM-ACCS, *EQPM-DOOR",
                Str2 = "*EQPM-ACCS, *EQPM-DOOR",
                LongStr = "*EQPM-ACCS, *EQPM-DOOR"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParamName = "LAYER_TYPE",
                Int1 = 2,
                Str1 = "*EQPM-ACCS-MANT",
                Str2 = "*EQPM-ACCS-MANT",
                LongStr = "*EQPM-ACCS-MANT"
            });
            return _configs;
        }
    }

    /// <summary> Simulates a Ruls data source, which ... </summary>
    [Export(typeof(IRuleRepository))]
    public class RuleRepository : IRuleRepository
    {
        public RuleRepository(
            ConfigRepository configRepository,
            LayoutRepository layoutRepository,
            UserSettingsRepository userSettingsRepository)
        {
            Configs = configRepository;
            Layouts = layoutRepository;
            UserSettings = userSettingsRepository;
        }

        public RuleRepository() { }

        [Import(typeof(IConfigRepository))]
        internal ConfigRepository Configs { get; set; }

        [Import(typeof(ILayoutRepository))]
        internal LayoutRepository Layouts { get; set; }

        [Import(typeof(IUserSettingRepository))]
        internal UserSettingsRepository UserSettings { get; set; }

        public IEnumerable<IRule> GetRules(string configSetName)
        {
            var results = new List<Rule>();
            Rule.RuleIndexesReset();
            Rule.LsdsTypeFilterOn = new[] { typeof(BlockReference) };

            var cfg = Configs.GetConfigs().FirstOrDefault(x => x.ConfigSetName == configSetName);
            if (cfg != null)
            {
                Rule.LsdsAttributePatternOn = cfg.LayoutOptions
                    .Where(x => x.ParameterName == "TOOL_NAME_ATTRIBUTE").Select(y => y.Value).ToArray();

                cfg.LayoutOptions.Where(x => x.ParameterName == "FRAME_TYPE_ID").ToList()
                    .ForEach(config =>
                        {
                            var layerTypes = cfg.LayoutOptions
                                .Where(x => x.ParameterName == "LAYER_TYPE" && x.Key.ToString() == config.Key.ToString())
                                .Select(x => x.Value).ToArray();

                            if (Convert.ToByte(config.Key) == 1 && !layerTypes.Contains("0")) //"Physical"
                            {
                                Array.Resize(ref layerTypes, layerTypes.Length + 1);
                                layerTypes[layerTypes.Length - 1] = "0";
                            }

                            if (layerTypes.Any())
                                results.Add(new Rule
                                    {
                                        Comment = "no comment",
                                        Name = "FRAME_TYPE_ID=" + config.Key.ToString().Trim(),
                                        ColorIndex = 0,
                                        isTypeFilterParent = true,
                                        LayerTypeId = Convert.ToByte(config.Key),

                                        //Removing not conventional simbols from layer names 
                                        LayerDestination = "." + ProjectManager.Name.ToUpper() + "-" + config.ConfigSetName.ToUpper() + "-" + config.Value.Trim().Replace(" ", "-").Replace("/", "-").ToUpper(),
                                        LayerPatternOn = layerTypes,
                                        LineType = ToolsManager.GetLineType("HIDDEN2"), //LineTypes.DOT
                                        LineTypeScale = 15,
                                        ParameterList = config.Value,
                                        TypeFilterOn = new[] { typeof(Curve) },
                                    });
                        });
            }
            return results;
        }

        /// <summary> Reset init </summary>
        public List<Rule> MakeRules(IConfigRepository configs, string configSetName)
        {
            var retval = new List<Rule>();
            //configs.SetConfigFilterFor(x => x.ConfigSetName == configSetName);

            Rule.RuleIndexesReset(); //ConfigManager.Rules.Clear();
            Rule.LsdsTypeFilterOn = new[] { typeof(BlockReference) };

            var cfg = configs.GetConfigs().FirstOrDefault(x => x.ConfigSetName == configSetName);
            if (cfg != null)
            {
                Rule.LsdsAttributePatternOn = cfg.LayoutOptions.Where(x => x.ParameterName == "TOOL_NAME_ATTRIBUTE").Select(y => y.Value).ToArray();
            }

            //Rule.LsdsAttributePatternOn = configs.GetConfigs().Where(x => x.ConfigSetName == configSetName)
            //    .Select(y => y.LayoutOptions)); //.Where(y1 => y1 == "TOOL_NAME_ATTRIBUTE")
            //Rule.LsdsLayoutCatalogOptions = configs.GetLayoutCatalogOptions().ToList();
            //Rule.LsdsLayoutCatalogSites = configs.GetLayoutCatalogSites().ToList();

            var frms = cfg.LayoutOptions.Where(x => x.ParameterName == "FRAME_TYPE_ID").ToList();

            frms.ForEach(config =>
                {
                    var layerTypes = cfg.LayoutOptions.Where(x => x.ParameterName == "LAYER_TYPE").Select(y => y.Key.ToString()).ToArray();

                    retval.Add(new Rule
                        {
                            Comment = "no comment",
                            Name = config.ParameterName,
                            ColorIndex = 0,
                            isTypeFilterParent = true,
                            LayerTypeId = Convert.ToByte(config.Key),
                            LayerDestination = "_" + config.Key.ToString().Trim().Replace(" ", "_").Replace("/", "-"),
                            //Removing not conventional simbols of layer names 
                            LayerPatternOn = layerTypes,
                            LineType = ToolsManager.GetLineType("HIDDEN2"), //LineTypes.DOT
                            LineTypeScale = 15,
                            ParameterList = config.Value,
                            TypeFilterOn = new[] { typeof(Curve) },
                        });
                });

            configs.SetConfigFilterFor(null);
            return retval;
        }

        //public List<LayerItem> GetLayers()
        //{
        //    return Rules.Cast<IRule>()
        //               .Select(r => new LayerItem 
        //                    { Color = Color.FromColorIndex(ColorMethod.ByAci, (short)r.ColorIndex), Name = r.LayerDestination })
        //               .Distinct()
        //               .ToList();
        //}
    }

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    [Export(typeof(IUserSettingRepository))]
    public class UserSettingsRepository : IUserSettingRepository
    {
        public IEnumerable<BaseUserSetting> GetUserSettings()
        {
            return new List<BaseUserSetting>() { UserSetting.Load() };
        }

        public void SaveSettings(IEnumerable<BaseUserSetting> userSettings)
        {
            userSettings.ToList()[0].Save();
        }
    }
}
