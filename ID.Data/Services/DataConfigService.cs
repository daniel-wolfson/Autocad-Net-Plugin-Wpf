using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;

namespace Intellidesk.Data.Services
{
    public class DataConfigService : Service<Config>, IDataConfigService
    {
        //private readonly IRepository<Config> _repository;
        private IEnumerable<Config> _items;
        private readonly IRepository<Config> _repository;

        //public ConfigService(IRepository<Config> repository)
        //    : base(repository)
        //{
        //    _repository = repository;
        //}
        public DataConfigService(IUnitOfWorkAsync uow) : base(uow)
        {
            _repository = uow.Repository<Config>();
            _items = _repository.Queryable()
                .WhenEmpty(this.CreateInstanceByDefault)
                .ToList();
            _items = GetConfigs();
        }

        public IEnumerable<Config> GetConfigs(bool fromCache = true)
        {
            if (!fromCache || _items.Any())
                return _items.ToList();

            return new List<Config>() { this.CreateInstanceByDefault() };
            //IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            //var result = pluginSettings.IsDemo || !base.Repository.Queryable().Any() 
            //    ? MakeDemoData()
            //    : GetConfigGroups(base.Repository.Queryable().ToList());
            //return result;
        }

        public ObservableCollection<Config> GetItems()
        {
            return _items.ToItems();
        }
        public ObservableCollection<Config> GetCurrent()
        {
            return _items.ToItems();
        }

        private IEnumerable<Config> GetConfigGroups(List<Config> configs)
        {
            var grps = configs.GroupBy(x => x.ConfigSetName).Select(grp => grp.FirstOrDefault());
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
    }
}