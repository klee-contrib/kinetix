using Kinetix.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.Web.Filters
{
    /// <summary>
    /// Filtre pour gérer la transaction dans MVC / Razor Pages.
    /// </summary>
    public class TransactionFilter<TDbContext> : IActionFilter, IPageFilter
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        private readonly TransactionScopeManager _transactionScopeManager;
        private ServiceScope _scope;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public TransactionFilter(TDbContext context, TransactionScopeManager transactionScopeManager)
        {
            _context = context;
            _transactionScopeManager = transactionScopeManager;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
            {
                _scope.Complete();
                _context.Database.CommitTransaction();
            }
            else
            {
                _context.Database.RollbackTransaction();
            }

            _scope.Dispose();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _scope = _transactionScopeManager.EnsureTransaction();
            _context.Database.BeginTransaction();
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            if (context.ModelState.IsValid)
            {
                _scope.Complete();
                _context.Database.CommitTransaction();
            }
            else
            {
                _context.Database.RollbackTransaction();
            }

            _scope.Dispose();
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            _context.Database.BeginTransaction();
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }
}