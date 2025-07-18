using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Intellidesk.AcadNet.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.AcadNet.Data.Models;
using Intellidesk.AcadNet.Data.Repositories;
using Intellidesk.Infrastructure;
using Microsoft.Practices.Unity;

namespace Intellidesk.AcadNet.Data.Services
{
    public class DataConfigService : Service<Config>, IDataConfigService
    {
        //private readonly IRepository<Config> _repository;
        private IEnumerable<Config> _items = new List<Config>();
        private IPluginManager _pluginManager;
        //public ConfigService(IRepository<Config> repository)
        //    : base(repository)
        //{
        //    _repository = repository;
        //}
        public DataConfigService(IUnitOfWorkAsync uow, IUnityContainer container)
            : base(uow.RepositoryAsync<Config>())
        {
            //_repository = uow.RepositoryAsync<Config>();
            _pluginManager = container.Resolve<IPluginManager>();
            _items = GetConfigs();
        }

        public IEnumerable<Config> GetConfigs()
        {
            var result = _pluginManager.IsDemo || !base.Repository.Queryable().Any() 
                ? MakeDemoData()
                : GetConfigGroups(base.Repository.Queryable().ToList());
            return result;
        }

        private IEnumerable<Config> GetConfigGroups(List<Config> configs)
        {
            var grps = configs.GroupBy(x => x.ConfigSetName).Select(grp => grp.First());
            var configGroups = grps as IList<Config> ?? grps.ToList();
            foreach (var group in configGroups)
            {
                var prms = configs.Select(x => x.ParameterName).Distinct().ToList();
                foreach (var par in prms)
                {
                    //group.LayoutOptions.AddRange(GetLayoutDictionaries(group.ConfigSetName, par));
                }
            }
            return configGroups;
        }

        public IEnumerable<string> GetConfigNames()
        {
            return _items.Select(y => y.ConfigSetName).Distinct();
        }

        public ObservableCollection<Config> GetItems()
        {
            return _items.ToItems();
        }
        public ObservableCollection<Config> GetCurrent()
        {
            return _items.ToItems();
        }
        public IEnumerable<LayoutDictionary> GetLayoutDictionaries(string configSetName, string paramName)
        {
            switch (paramName)
            {
                case "FRAME_TYPE_ID":
                    return _items.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1.ToString(), Value = opt.Str1, ParameterName = paramName });
                case "LAYOUT_CATALOG_OPTIONS":
                    return _items.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str2, ParameterName = paramName });
                case "LAYOUT_CATALOG_SITE":
                    return _items.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.LongStr, ParameterName = paramName });
                case "LAYER_TYPE":
                    return _items.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Int1.ToString(), Value = opt.Str1, ParameterName = paramName });
                case "TOOL_NAME_ATTRIBUTE":
                    return _items.Where(x => x.ConfigSetName == configSetName && x.ParameterName == paramName)
                          .Select(opt => new LayoutDictionary { ConfigSetName = opt.ConfigSetName, Key = opt.Str1, Value = opt.Str1, ParameterName = paramName });

            }
            return new List<LayoutDictionary>();
        }

        public IEnumerable<Config> SetConfigFilterFor(Func<Config, bool> exp)
        {
            _items = exp != null ? _items.Where(exp) : _items = GetConfigs();
            return _items;
        }
        public bool IsEmpty()
        {
            return _items.Any();
        }

        public List<Config> MakeDemoData()
        {
            var configs = new List<Config>
            {
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYOUT_CATALOG_OPTIONS",
                    Int1 = 1,
                    Str1 = "ACCESS_TYPE",
                    Str2 = "typ1, typ2",
                    LongStr = "typ1, typ2"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYOUT_CATALOG_SITE",
                    Int1 = 1,
                    Str1 = "LC - Lachish Israel",
                    Str2 = "site1, site2",
                    LongStr = "site1, site2"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYOUT_CATALOG_OPTIONS",
                    Int1 = 1,
                    Str1 = "LAYOUT_CONTENT",
                    Str2 = "content1, content2",
                    LongStr = "content1, content2"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYOUT_CATALOG_OPTIONS",
                    Int1 = 1,
                    Str1 = "CONTENT_TYPE",
                    Str2 = "type1, type2",
                    LongStr = "type1, type2"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYOUT_CATALOG_OPTIONS",
                    Int1 = 1,
                    Str1 = "PROCESSES",
                    Str2 = "proc1, proc2",
                    LongStr = "proc1, proc2"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "TOOL_NAME_ATTRIBUTE",
                    Int1 = 1,
                    Str1 = "ENTITY_CODES",
                    Str2 = "ENTITY_CODES",
                    LongStr = "ENTITY_CODES"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "TOOL_NAME_ATTRIBUTE",
                    Int1 = 1,
                    Str1 = "ENTITY_CODE",
                    Str2 = "ENTITY_CODE",
                    LongStr = "ENTITY_CODE"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "TOOL_NAME_ATTRIBUTE",
                    Int1 = 1,
                    Str1 = "TOOL_ID",
                    Str2 = "TOOL_ID",
                    LongStr = "TOOL_ID"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "FRAME_TYPE_ID",
                    Int1 = 1,
                    Str1 = "Fysical",
                    Str2 = "Fysical",
                    LongStr = "Fysical"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "FRAME_TYPE_ID",
                    Int1 = 2,
                    Str1 = "Clearence",
                    Str2 = "Clearence",
                    LongStr = "Clearence"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYER_TYPE",
                    Int1 = 1,
                    Str1 = "*EQPM-ACCS, *EQPM-DOOR",
                    Str2 = "*EQPM-ACCS, *EQPM-DOOR",
                    LongStr = "*EQPM-ACCS, *EQPM-DOOR"
                },
                new Config
                {
                    ConfigSetName = "Default",
                    ParameterName = "LAYER_TYPE",
                    Int1 = 2,
                    Str1 = "*EQPM-ACCS-MANT",
                    Str2 = "*EQPM-ACCS-MANT",
                    LongStr = "*EQPM-ACCS-MANT"
                }
            };
            //_configs.Clear();

            return configs;
        }
    }
}