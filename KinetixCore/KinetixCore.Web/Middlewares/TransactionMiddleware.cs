using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Filters
{
    public class TransactionMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                var res = next(context);
                scope.Complete();
                scope.Dispose();
                return res;
            }
        }
    }
}
