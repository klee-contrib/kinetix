using System.Collections.Generic;
using Kinetix.Services.DependencyInjection.Interceptors;
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
        private readonly IEnumerable<IOnBeforeCommit> _onBeforeCommits;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public TransactionFilter(TDbContext context, IEnumerable<IOnBeforeCommit> onBeforeCommits)
        {
            _context = context;
            _onBeforeCommits = onBeforeCommits;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
            {
                foreach (var onBeforeCommit in _onBeforeCommits)
                {
                    onBeforeCommit.OnBeforeCommit();
                }

                _context.Database.CommitTransaction();
            }
            else
            {
                _context.Database.RollbackTransaction();
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _context.Database.BeginTransaction();
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            if (context.ModelState.IsValid)
            {
                _context.Database.CommitTransaction();
            }
            else
            {
                _context.Database.RollbackTransaction();
            }
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            _context.Database.BeginTransaction();
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }
}