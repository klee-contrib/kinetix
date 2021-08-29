using Kinetix.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters
{
    /// <summary>
    /// Filtre pour gérer la transaction dans MVC / Razor Pages.
    /// </summary>
    public class TransactionFilter : IActionFilter, IPageFilter
    {
        private readonly TransactionScopeManager _transactionScopeManager;
        private ServiceScope _scope;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public TransactionFilter(TransactionScopeManager transactionScopeManager)
        {
            _transactionScopeManager = transactionScopeManager;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
            {
                _scope.Complete();
            }

            _scope.Dispose();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _scope = _transactionScopeManager.EnsureTransaction();
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            if (context.ModelState.IsValid)
            {
                _scope.Complete();
            }

            _scope.Dispose();
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            _scope = _transactionScopeManager.EnsureTransaction();
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }
}