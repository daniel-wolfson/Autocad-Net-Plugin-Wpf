using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.General;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;

namespace Intellidesk.Data.Services
{
    /// <summary>
    ///     All methods that are exposed from Repository in Service are overridable to add business logic,
    ///     business logic should be in the Service layer and not in repository for separation of concerns.
    /// </summary>
    public class LayoutService : Service<ILayout>, ILayoutService
    {
        private readonly IRepository<ILayout> _repository = null;
        private IEnumerable<ILayout> _items;

        public LayoutService(IUnitOfWorkAsync uow)
            : base(uow)
        {
            _repository = uow.RepositoryAsync<ILayout>();

            //var sql = "SELECT COUNT(*) FROM Layouts";
            //var total = _repository.SelectQuery(sql).Single();
            //total = _repository.SqlQuery<int>(sql).Single();

            _items = GetLayouts()//_repository.Queryable()
                .WhenEmpty(this.CreateInstanceByDefault)
                .Select(x => x).AsEnumerable();
        }

        public decimal CustomerOrderTotalByYear(string customerId, int year)
        {
            return _repository.GetCustomerOrderTotalByYear(customerId, year);
        }

        public IEnumerable<ILayout> LoadLayoutByFileName(string companyName)
        {
            return _repository
                .Queryable()
                .Where(x => x.CADFileName.Contains(companyName))
                .AsEnumerable();
        }

        public IEnumerable<ILayout> GetLayouts(bool fromCache = true)
        {
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();

            if (!pluginSettings.IsDemo && !fromCache && _items.Any())
                return _items.Where(x => x.Visible).ToList();

            List<ILayout> layouts = new List<ILayout>();
            if (pluginSettings.WorkItems != null && pluginSettings.WorkItems.Any())
            {
                IEnumerable<string> groups = pluginSettings.WorkItems.GroupBy(g => g.FullPath).Select(x => x.Key);
                foreach (var group in groups)
                {
                    List<IWorkItem> groupItems = pluginSettings.WorkItems.Where(x => x.FullPath == group).ToList();
                    layouts.Add(new ILayout(group, groupItems));
                }
            }

            return layouts;  //new List<Layout>() { this.CreateInstanceByDefault() };
        }

        public override ILayout CreateInstanceByDefault()
        {
            //return base.CreateInstance();
            return new ILayout("");
        }

        public override ILayout Clone(ILayout obj)
        {
            var clone = new ILayout(obj.LayoutName)
            {
                AccessType = obj.AccessType,
                BuildingLevels = obj.BuildingLevels,
                CADFileName = obj.CADFileName,
                Comment = obj.Comment,
                CreatedBy = obj.CreatedBy,
                DateCreated = obj.DateCreated,
                DateModified = obj.DateCreated,
                FSA = obj.FSA,
                LayoutContents = obj.LayoutContents,
                LayoutID = obj.LayoutID,
                LayoutName = obj.LayoutName,
                LayoutState = obj.LayoutState,
                LayoutType = obj.LayoutType,
                LayoutVersion = obj.LayoutVersion,
                ModifiedBy = obj.ModifiedBy,
                SiteName = obj.SiteName
            }; //var copy = (BaseLayout)Activator.CreateInstance(this.GetType());
            return clone;
        }

        public void AddAndSave(string fullPath, string work, string commandType)
        {
            var items = new ObservableCollection<ILayout>();
            if (!string.IsNullOrEmpty(fullPath))
            {
                IPluginSettings PluginSettings = Plugin.GetService<IPluginSettings>();
                PluginSettings.WorkItems.Add(new WorkItem(fullPath, commandType, work));
                PluginSettings.Save();
            }
        }
    }
}