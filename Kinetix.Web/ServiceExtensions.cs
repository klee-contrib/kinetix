using Kinetix.Web.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web
{
    public static class ServiceExtensions
    {
        public static void AddWeb<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddTransient<CultureFilter>();
            services.AddTransient<ExceptionFilter>();
            services.AddTransient<TransactionFilter<TDbContext>>();
        }
    }
}
