using System.Linq;
using Kinetix.Services;
using Kinetix.Web.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddWeb<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddScoped<CultureFilter>();
            services.AddScoped<ExceptionFilter>();
            services.AddScoped<TransactionFilter<TDbContext>>();
            services.AddScoped<ReferenceCheckerFilter>();

            if (!services.Any(service => service.ServiceType == typeof(IReferenceManager)))
            {
                services.AddSingleton<IReferenceManager, ReferenceManager>();
            }

            return services;
        }
    }
}
