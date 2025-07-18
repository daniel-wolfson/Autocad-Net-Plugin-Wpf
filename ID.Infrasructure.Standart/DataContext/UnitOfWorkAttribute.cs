using ID.Infrastructure.Core;
using ID.Infrastructure.DataContext;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ID.Infrastructure.Filters
{
    public class UnitOfWorkAttribute : IActionFilter
    {
        public IUnitOfWork UoW { get; set; }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            UoW = GeneralContext.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            System.Diagnostics.Trace.WriteLine("Scoped UoW " + UoW.SessionId);

            //Uow.Commit(); here you would commit
            System.Diagnostics.Trace.WriteLine("GlobalConfig UoW " + UoW.SessionId);
        }
    }
}
