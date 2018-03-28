using Kinetix.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddWeb(this IServiceCollection services)
        {
            services.AddTransient<CultureFilter>();
            services.AddTransient<ExceptionFilter>();
            //services.AddTransient<TransactionMiddleware>();
            return services;
        }
    }
}
