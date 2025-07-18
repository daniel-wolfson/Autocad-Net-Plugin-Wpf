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
using AcadNet.Data.Repository;
using Layout = AcadNet.Data.Models.Gis.Layout;

namespace AcadNet.Data.Models.Gis
{
    public interface ILayoutRepository : IRepository<Layout>
    {
        void SetLayouts(IEnumerable<Layout> layouts);

        //ILayout GetById(long id)
        //{
        //    return _dbset.Include(x => x.Country).Where(x => x.Id == id).FirstOrDefault();
        //}
    }

    public interface IUserStatesRepository
    {
        //IEnumerable<State> GetStates();
        //void SetStates(IEnumerable<State> states);
        void SetUpdate<T>(T entity, params string[] properties) where T : class;
        void SetUpdate<T>(T entity, string propertyName, object value) where T : class;
    }

    public interface ILayoutFilterRepository
    {
        IEnumerable<LayoutFilter> GetLayoutFilters();
        void SaveSettings(IEnumerable<LayoutFilter> layoutFilters);
    }

    /// <summary> Simulates a Layout data source, which would normally come from a database </summary>
    //[Export(typeof(IUserStatesRepository))]
    //public class StateRepository : IUserStatesRepository
    //{
    //    private static readonly string _filename = @"Models\States.xml";

    //    // Get Layouts Settings
    //    public IEnumerable<State> GetStates()
    //    {
    //        if (!File.Exists(_filename))
    //        {
    //            throw new FileNotFoundException("DataSource file could not be found", _filename);
    //        }

    //        var items = System.Xml.Linq.XDocument.Load(_filename).Root.Elements("Item").Select(
    //            x => new State() {
    //                AsMade = (string)x.Element("AsMade"), 
    //                Lon = (float)x.Element("Lon"), 
    //                Lat = (float)x.Element("Lat"),
    //                DateCreated = Convert.ToDateTime(x.Element("DateCreated"))
    //            });
    //        return items;
    //    }

    //    // Saves layouts
    //    public void SetStates(IEnumerable<State> states)
    //    {
    //        if (states == null)
    //            throw new ArgumentNullException("states");

    //        new System.Xml.Linq.XDocument(states).Save(_filename);
    //    }

    //    public void SetUpdate<T>(T entity, string propertyName, object value) where T : class
    //    {
    //        using (var context = DataToolsManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.Configuration.ValidateOnSaveEnabled = false;
    //                    if (context.Entry(entity).State == EntityState.Detached)
    //                        context.Set<T>().Attach(entity);

    //                    entity.GetType().GetProperty(propertyName).SetValue(entity, value, null);
    //                    context.Entry(entity).Property(propertyName).IsModified = true;

    //                    //context.Entry(entity).State = EntityState.Modified;
    //                    context.SaveChanges();
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    public void SetUpdate<T>(T entity, params string[] properties) where T : class
    //    {
    //        using (var context = DataToolsManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.Configuration.ValidateOnSaveEnabled = false;
    //                    if (context.Entry(entity).State == EntityState.Detached)
    //                        context.Set<T>().Attach(entity);

    //                    if (properties.Length > 0)
    //                        foreach (var property in properties)
    //                            context.Entry(entity).Property(property).IsModified = true;

    //                    //context.Entry(entity).State = EntityState.Modified;
    //                    context.SaveChanges();
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    // Get Layouts
    //    public IEnumerable<State> LoadStates()
    //    {
    //        var states = new List<State>();
    //        using (var context = DataToolsManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.State.ToList().ForEach(x =>
    //                        states.Add(new State
    //                        {
    //                            AsMade = x.AsMade,
    //                            Lat = x.Lat,
    //                            Lon = x.Lon,
    //                            DateCreated = x.DateCreated
    //                        }));
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //        return states;
    //    }
    //}

    /// <summary> Simulates a Layout data source, which would normally come from a database </summary>
    [Export(typeof(ILayoutRepository))]
    public class LayoutRepository : BaseRepository<Layout>, ILayoutRepository
    {
        private static readonly string _filename = @"Models\LayoutSettings.xml";

        public LayoutRepository(GisContext context) : base(context) { }

        public override IEnumerable<Layout> GetAll()
        {
            if (NetTools.ProjectManager.IsDemo)
                return (IEnumerable<Layout>)MakeDemoData();

            return _dbset.AsEnumerable<Layout>();
        }

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
            using (var context = DataToolsManager.CreateContext<GisContext>())
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
                            //_configs = context.LSDS_Config.Select(x =>
                            //new Config { ConfigSetName = x.ConfigSetName, ParamName = x.ParameterName, Int1 = x.Int1, Str1 = x.Str1, Str2 = x.Str2, LongStr = x.LongStr }).ToList();
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

    /// <summary> Simulates a UserSettings data source, which ... </summary>
    [Export(typeof(ILayoutFilterRepository))]
    public class LayoutFilterRepository : ILayoutFilterRepository
    {
        public IEnumerable<LayoutFilter> GetLayoutFilters()
        {
            return new List<LayoutFilter>() { LayoutFilter.Load() };
        }

        public void SaveSettings(IEnumerable<LayoutFilter> userSettings)
        {
            userSettings.ToList()[0].Save();
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
            var results = new List<IRule>();
            return results;
        }

        /// <summary> Reset init </summary>
        public List<Rule> MakeRules(IConfigRepository configs, string configSetName)
        {
            var retval = new List<Rule>();
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

    //public class EfRepository : IRepository
    //{
    //    #region Private fields
    //    private Lsds2Context _context;
    //    #endregion
    //    #region Constructors

    //    public EfRepository(Lsds2Context context)
    //    {
    //        _context = context;
    //    }
    //    #endregion

    //    #region Implementation of IRepository
    //    public IQueryable<T> Query<T>() where T : class
    //    {
    //        return _context.Set<T>();
    //    }
    //    public void Save<T>(T entity) where T : class
    //    {
    //        _context.Set<T>().Add(entity);
    //    }
    //    //public void Update1<T>(T entity) where T : class
    //    //{
    //    //    _context.Set<T>().Attach(entity);
    //    //    _context.Entry<T>(entity).State = EntityState.Modified;
    //    //}

    //    public void Update<T>(T entity) where T : class
    //    {
    //        if (_context.Entry(entity).State == EntityState.Detached)
    //        {
    //            _context.Set<T>().Attach(entity);
    //        }
    //        _context.Entry<T>(entity).State = EntityState.Modified;
    //    }

    //    public void Delete<T>(T entity) where T : class
    //    {
    //        if (_context.Entry(entity).State == System.Data.EntityState.Detached)
    //        {
    //            _context.Set<T>().Attach(entity);
    //        }
    //        _context.Entry<T>(entity).State = EntityState.Deleted;
    //    }

    //    //public void Delete1<T>(T entity) where T : class
    //    //{
    //    //    _context.Set<T>().Attach(entity);
    //    //    _context.Entry<T>(entity).State = EntityState.Deleted;
    //    //}

    //    public void DeleteById<T>(int entityId) where T : class
    //    {
    //        var entity = _context.Set<T>().Find(entityId);
    //        _context.Entry<T>(entity).State = EntityState.Deleted;
    //    }

    //    public void SaveChanges()
    //    {
    //        _context.SaveChanges();
    //    }

    //    //public IGenericTransaction BeginTransaction()
    //    //{
    //    //    return new EfTransaction(this);
    //    //}

    //    //public void EndTransaction(IGenericTransaction transaction)
    //    //{
    //    //    if (transaction != null)
    //    //    {
    //    //        transaction.Dispose();
    //    //        transaction = null;
    //    //    }
    //    //}

    //    public void Dispose()
    //    {
    //        if (this._context != null)
    //        {
    //            this._context.SaveChanges();
    //            (this._context as IDisposable).Dispose();
    //            this._context = null;
    //        }
    //    }

    //    #endregion
    //}
}
