using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.Web.Filters
{
    /// <summary>
    /// Filtre pour gérer la transaction dans MVC.
    /// </summary>
    public class TransactionFilter<TDbContext> : IActionFilter
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public TransactionFilter(TDbContext context)
        {
            _context = context;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _context.Database.CommitTransaction();
        }

        /// <summary>
        /// Action.
        /// </summary>
        /// <param name="context">Current context.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _context.Database.BeginTransaction();
        }
    }
}