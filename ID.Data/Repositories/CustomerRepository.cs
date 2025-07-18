using System.Collections.Generic;
using System.Linq;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Repositories
{
    public static class CustomerRepository
    {
        public static decimal GetCustomerOrderTotalByYear(this IRepository<Models.Cad.ILayout> repository, string customerId, int year)
        {
            return repository
                .Queryable()
                .Where(c => c.LayoutID.ToString() == customerId)
                .ToList().Count;
            //.SelectMany(c => c.Orders.Where(o => o.OrderDate != null && o.OrderDate.Value.Year == year))
            //.SelectMany(c => c.OrderDetails)
            //.Select(c => c.Quantity*c.UnitPrice)
            //.Sum();
        }

        public static IEnumerable<Models.Cad.ILayout> CustomersByCompany(this IRepository<Models.Cad.ILayout> repository, string companyName)
        {
            return repository
                .Queryable()
                .Where(x => x.LayoutName.Contains(companyName))
                .AsEnumerable();
        }

        public static IEnumerable<Models.Cad.ILayout> GetCustomerOrder(this IRepository<Models.Cad.ILayout> repository, string country)
        {
            var customers = repository.GetRepository<Models.Cad.ILayout>().Queryable();
            //var states = repository.GetRepository<State>().Queryable();

            //var query = from c in Layouts
            //            join o in States on new { a = c.LayoutId, b = c.LayoutName }
            //                equals new { a = o.StateID, b = country }
            //            select new State
            //            {
            //                StateId = c.StateID,
            //                FileName = c.FileName,
            //            };

            return customers; //query.AsEnumerable();
        }
    }
}