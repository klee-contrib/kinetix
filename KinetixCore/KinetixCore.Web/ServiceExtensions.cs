using Kinetix.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web
{
    public static class ServiceExtensions
    {
        public static void AddWeb(this IServiceCollection services)
        {
            services.AddTransient<CultureFilter>();
            services.AddTransient<ExceptionFilter>();
            services.AddTransient<TransactionMiddleware>();
        }
    }
}
