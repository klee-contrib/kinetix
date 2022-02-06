using Kinetix.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web;

public static class ServiceExtensions
{
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        return services
            .AddScoped<CultureFilter>()
            .AddScoped<ExceptionFilter>()
            .AddScoped<TransactionFilter>()
            .AddScoped<ReferenceCheckerFilter>()
            .AddScoped<UtcDateFilter>();
    }
}
