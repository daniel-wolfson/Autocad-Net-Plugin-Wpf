using System.Collections.Generic;

using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.AcadNet.Data.Models.Intel;

namespace Intellidesk.AcadNet.Data.Repositories.Intel
{
    public interface ILayoutRepository : IRepository<Layout>
    {
        IEnumerable<Layout> GetLayouts();
        void SetLayouts(IEnumerable<Layout> layouts);
        //void SetUpdate<T>(T entity, params string[] properties) where T : class;
        //void SetUpdate<T>(T entity, string propertyName, object value) where T : class;
        //ILayout GetById(long id)
        //{
        //    return _dbset.Include(x => x.Country).Where(x => x.Id == id).FirstOrDefault();
        //}
    }
}
