using Kinetix.Search.Core.DocumentModel;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search.Core;

/// <summary>
/// Enregistre Kinetix.Search dans ASP.NET Core.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        return services
            .AddSingleton<DocumentDescriptor>()
            .AddScoped<ITransactionContextProvider, IndexingTransactionContextProvider>()
            .AddScoped<IndexManager>();
    }
}
