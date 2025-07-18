using ID.Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Transactions;

namespace ID.Infrastructure.Filters
{
    public class ApiTransaction : ActionFilterAttribute
    {
        private const string TransactionKeyToken = "ApiTransactionToken";
        private const string TransactionKeyScope = "ApiTransactionScope";
        private string TransactionToken { get; set; }

        /// <summary> Retrieve a transaction propagation token, create a transaction scope and promote the current transaction to a distributed transaction. </summary>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.HttpContext.Items.ContainsKey(TransactionKeyToken))
                TransactionToken = actionContext.HttpContext.Items[TransactionKeyToken].ToString();
            else
                TransactionToken = $"{Transaction.Current?.TransactionInformation.LocalIdentifier ?? Util.CreateGuid()}${actionContext.HttpContext.Request.Path}";

            //var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            //actionContext.HttpContext.Items.Add(TransactionKeyScope, transactionScope);
            actionContext.HttpContext.Items.Add(TransactionKeyToken, TransactionToken);
        }

        /// <summary> Rollback or commit transaction. </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.HttpContext.Items.ContainsKey(TransactionKeyToken))
            {
                //var transactionScope = actionExecutedContext.HttpContext.Items[TransactionKeyScope] as TransactionScope;
                //if (transactionScope != null)
                //{
                //    if (actionExecutedContext.Exception != null)
                //        Transaction.Current.Rollback();
                //    else
                //        transactionScope.Complete();
                //    transactionScope.Dispose();
                //}
                //actionExecutedContext.HttpContext.Items.Remove(TransactionKeyScope);
                actionExecutedContext.HttpContext.Items.Remove(TransactionKeyToken);
            }
        }
    }
}
