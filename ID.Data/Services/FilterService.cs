using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.Data.Services
{
    /// <summary>
    ///     Add any custom business logic (methods) here
    /// </summary>
    public interface IFilterService : IService<Filter>
    {
        Filter GetFilterByName(string name);
        IEnumerable<Filter> GetFilters(bool fromCache = true);
    }

    /// <summary>
    ///     All methods that are exposed from Repository in Service are overridable to add business logic,
    ///     business logic should be in the Service layer and not in repository for separation of concerns.
    /// </summary>
    public class FilterService : Service<Filter>, IFilterService
    {
        private readonly List<Filter> _items;
        private readonly IRepository<Filter> _repository;

        public FilterService(IUnitOfWorkAsync uow) : base(uow)
        {
            _repository = uow.Repository<Filter>();
            _items = _repository.Queryable()
                .WhenEmpty(this.CreateInstanceByDefault)
                .ToList();
        }

        public Filter GetFilterByName(string name)
        {
            return GetFilters().FirstOrDefault(x => x.FilterName == name);
        }
        public IEnumerable<Filter> GetFilters(bool fromCache = true)
        {
            if (!fromCache || _items.Any())
                return _items.ToList();

            return new List<Filter>() { this.CreateInstanceByDefault() };
        }

        public ObservableCollection<Filter> GetItems()
        {
            return GetFilters().ToItems();
        }

        public override Filter CreateInstanceByDefault()
        {
            return new Filter() { };
        }
    }
}