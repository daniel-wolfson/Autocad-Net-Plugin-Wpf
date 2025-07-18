using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;

using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.AcadNet.Data.Models;
using Intellidesk.AcadNet.Data.Models.Intel;
using Intellidesk.AcadNet.Data.Repositories.EF6.DataContext;
using Intellidesk.AcadNet.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.AcadNet.Data.Repositories.Infrastructure;
using Intellidesk.AcadNet.Data.Services;
using Intellidesk.NetTools;
using Layout = Intellidesk.AcadNet.Data.Models.Intel.Layout;
using Rule = Intellidesk.AcadNet.Data.Models.Intel.Rule;

namespace Intellidesk.AcadNet.Data.Repositories.Intel
{
   /// <summary> Simulates a Layout data source, which would normally come from a database </summary>
    //[Export(typeof(ILayoutRepository))]
    public class LayoutRepository : Repository<Layout>, ILayoutRepository
    {
        private static readonly string _filename = @"Models\LayoutSettings.xml";

        public LayoutRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

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

        public IEnumerable<Layout> GetAll_Old()
        {
            //if (ProjectManager.IsDemo) return MakeDemoData();

            var layouts = new List<Layout>();
            //using (var context = DataManager.CreateContext<Lsds2Context>())
            //{
            //    //if (ProjectManager.IsDemo) layouts = (List<Layout>)MakeDemoData();

            //    if (context != null)
            //    {
            //        try
            //        {
            //            context.Layouts.ToList().ForEach(x =>
            //                layouts.Add(new Layout
            //                {
            //                    AccessType = x.AccessType,
            //                    BuildingLevels = x.BuildingLevels,
            //                    CADFileName = x.CADFileName,
            //                    CreatedBy = x.CreatedBy,
            //                    Comment = x.Comment,
            //                    DateCreated = x.DateCreated,
            //                    DateModified = x.DateModified,
            //                    FSA = x.FSA ?? false,
            //                    LayoutID = x.LayoutID,
            //                    LayoutName = x.LayoutName,
            //                    LayoutType = x.LayoutType,
            //                    LayoutContents = x.LayoutContents,
            //                    LayoutVersion = x.LayoutVersion,
            //                    LayoutState = x.LayoutState,
            //                    ModifiedBy = x.ModifiedBy,
            //                    ProcessName1 = x.ProcessName1,
            //                    ProcessName2 = x.ProcessName2,
            //                    ProcessName3 = x.ProcessName3,
            //                    ProcessName4 = x.ProcessName4,
            //                    SiteName = x.SiteName,
            //                    WSPW1 = x.WSPW1,
            //                    WSPW2 = x.WSPW2,
            //                    WSPW3 = x.WSPW3,
            //                    WSPW4 = x.WSPW4,
            //                    Visible = x.Visible
            //                }));
            //        }
            //        catch (Exception)
            //        {
            //            //layouts = MakeDemoData();
            //        }
            //    }
            //}
            return layouts;
        }

        // Saves layouts
       public IEnumerable<Layout> GetLayouts()
       {
           if (ProjectManager.IsDemo)
               return (IEnumerable<Layout>) LayoutRepositoryExtensions.MakeLayoutSample<Layout>();

           return base.Queryable();
       }

       public void SetLayouts(IEnumerable<Layout> layouts)
        {
            if (layouts == null)
                throw new ArgumentNullException("layouts");

            new System.Xml.Linq.XDocument(layouts).Save(_filename);
        }

        public List<Layout> MakeDemoData()
        {
            return new List<Layout> { LayoutRepositoryExtensions.MakeLayoutSample<Layout>() };
        }

        public new void SetUpdate<T>(T entity, string propertyName, object value) where T : class
        {
            using (var context = DataManager.CreateContext<Lsds2Context>())
            {
                if (context != null)
                {
                    try
                    {
                        context.Configuration.ValidateOnSaveEnabled = false;
                        if (context.Entry(entity).State == EntityState.Detached)
                            context.Set<T>().Attach(entity);

                        entity.GetType().GetProperty(propertyName).SetValue(entity, value, null);
                        context.Entry(entity).Property(propertyName).IsModified = true;

                        //context.Entry(entity).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        public void SetUpdate<T>(T entity, params string[] properties) where T : class
        {
            using (var context = DataManager.CreateContext<Lsds2Context>())
            {
                if (context != null)
                {
                    try
                    {
                        context.Configuration.ValidateOnSaveEnabled = false;
                        if (context.Entry(entity).State == EntityState.Detached)
                            context.Set<T>().Attach(entity);

                        if (properties.Length > 0)
                            foreach (var property in properties)
                                context.Entry(entity).Property(property).IsModified = true;

                        //context.Entry(entity).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

       public ObjectState ObjectState { get; set; }
    }

    /// <summary> Simulates a Config data source, which ... </summary>
    //[Export(typeof(IConfigRepository))]
    public class ConfigRepository : Repository<Config>, IConfigRepository
    {
        private List<Config> _configs = new List<Config>();
        public Func<Config, bool> Exp = null;

        public ConfigRepository(IDataContextAsync context, IUnitOfWorkAsync uow) : base(context, uow) { }

        // Retrieves Configs
        public IEnumerable<Config> GetAll()
        {
            var grps = new List<Config>();
            using (var context = DataManager.CreateContext<Lsds2Context>())
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
                            new Config { ConfigSetName = x.ConfigSetName, ParameterName = x.ParameterName, Int1 = x.Int1, Str1 = x.Str1, Str2 = x.Str2, LongStr = x.LongStr }).ToList();
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
                            var prms = _configs.Select(x => x.ParameterName).Distinct().ToList();
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
            return GetAll().GroupBy(x => x.ConfigSetName).Select(grp => grp.First());
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
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1, Value = opt.Str1, ParameterName = paramName });
                case "LAYOUT_CATALOG_OPTIONS":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str2, ParameterName = paramName });
                case "LAYOUT_CATALOG_SITE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.LongStr, ParameterName = paramName });
                case "LAYER_TYPE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1, Value = opt.Str1, ParameterName = paramName });
                case "TOOL_NAME_ATTRIBUTE":
                    return _configs.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str1, ParameterName = paramName });

            }
            return new List<LayoutDictionary>();
        }

        public IEnumerable<Config> SetConfigFilterFor(Func<Config, bool> exp)
        {
            Exp = exp;
            _configs = exp != null ? _configs.Where(Exp).ToList() : _configs = GetAll().ToList();
            return _configs;
        }

        public bool IsEmpty()
        {
            return _configs.Any();
        }

        public List<Config> MakeDemoData()
        {
            _configs.Clear();
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "ACCESS_TYPE",
                Str2 = "typ1, typ2",
                LongStr = "typ1, typ2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYOUT_CATALOG_SITE",
                Int1 = 1,
                Str1 = "LC - Lachish Israel",
                Str2 = "site1, site2",
                LongStr = "site1, site2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "LAYOUT_CONTENT",
                Str2 = "content1, content2",
                LongStr = "content1, content2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "CONTENT_TYPE",
                Str2 = "type1, type2",
                LongStr = "type1, type2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYOUT_CATALOG_OPTIONS",
                Int1 = 1,
                Str1 = "PROCESSES",
                Str2 = "proc1, proc2",
                LongStr = "proc1, proc2"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "ENTITY_CODES",
                Str2 = "ENTITY_CODES",
                LongStr = "ENTITY_CODES"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "ENTITY_CODE",
                Str2 = "ENTITY_CODE",
                LongStr = "ENTITY_CODE"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "TOOL_NAME_ATTRIBUTE",
                Int1 = 1,
                Str1 = "TOOL_ID",
                Str2 = "TOOL_ID",
                LongStr = "TOOL_ID"
            });

            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "FRAME_TYPE_ID",
                Int1 = 1,
                Str1 = "Fysical",
                Str2 = "Fysical",
                LongStr = "Fysical"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "FRAME_TYPE_ID",
                Int1 = 2,
                Str1 = "Clearence",
                Str2 = "Clearence",
                LongStr = "Clearence"
            });

            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYER_TYPE",
                Int1 = 1,
                Str1 = "*EQPM-ACCS, *EQPM-DOOR",
                Str2 = "*EQPM-ACCS, *EQPM-DOOR",
                LongStr = "*EQPM-ACCS, *EQPM-DOOR"
            });
            _configs.Add(new Config
            {
                ConfigSetName = "Default",
                ParameterName = "LAYER_TYPE",
                Int1 = 2,
                Str1 = "*EQPM-ACCS-MANT",
                Str2 = "*EQPM-ACCS-MANT",
                LongStr = "*EQPM-ACCS-MANT"
            });
            return _configs;
        }

    }

    /// <summary> Simulates a Ruls data source, which ... </summary>
    //[Export(typeof(XmlRepository<Rule>))]
    public class RuleRepository : Repository<Rule>
    {
        public RuleRepository(IDataContextAsync context, IUnitOfWorkAsync uow)
            : base(context, uow)
        {
            ConfigsRepository = (ConfigRepository)uow.Repository<Config>();
            LayoutsRepository = (LayoutRepository)uow.Repository<Layout>();
            UserSettingsRepository = (UserSettingsRepository)uow.XmlRepository<UserSetting>();
        }


        [Import(typeof(IConfigRepository))]
        internal ConfigRepository ConfigsRepository { get; set; }

        [Import(typeof(Intellidesk.AcadNet.Data.Repositories.Intel.ILayoutRepository))]
        internal LayoutRepository LayoutsRepository { get; set; }

        [Import(typeof(IUserSettingRepository))]
        internal UserSettingsRepository UserSettingsRepository { get; set; }

        public IEnumerable<Rule> GetAll(string configSetName)
        {
            var results = new List<Rule>();
            Rule.RuleIndexesReset();
            Rule.LsdsTypeFilterOn = new[] { Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.BlockReference") }; //typeof(BlockReference)

            var cfg = ConfigsRepository.GetAll().FirstOrDefault(x => x.ConfigSetName == configSetName);
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
                                        //LineType = ToolsManager.GetLineType("HIDDEN2"), //LineTypes.DOT !!!!!!!!!!!!!
                                        LineTypeScale = 15,
                                        ParameterList = config.Value,
                                        TypeFilterOn = new[] { Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.Curve") }, //typeof(Curve)
                                    });
                        });
            }
            return results.AsEnumerable();
        }

        /// <summary> Reset init </summary>
        public List<Rule> MakeRules(IConfigRepository repository, string configSetName)
        {
            var retval = new List<Rule>();
            //configs.SetConfigFilterFor(x => x.ConfigSetName == configSetName);

            Rule.RuleIndexesReset(); //ConfigManager.Rules.Clear();
            Rule.LsdsTypeFilterOn = new[] { Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.BlockReference") };

            var cfg = repository.GetAll().FirstOrDefault(x => x.ConfigSetName == configSetName);
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
                            //LineType = ToolsManager.GetLineType("HIDDEN2"), //LineTypes.DOT  !!!!!!!!!!!!!!!!
                            LineTypeScale = 15,
                            ParameterList = config.Value,
                            TypeFilterOn = new[] { Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.Curve") },
                        });
                });

            //new ConfigService(repository).SetConfigFilterFor(null);
            //repository.SetConfigFilterFor(null);
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
}
