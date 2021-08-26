using Kinetix.Search.MetaModel;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSearch(this IServiceCollection services)
        {
            return services
                .AddSingleton<DocumentDescriptor>()
                .AddSingleton<ITransactionContextProvider, IndexingTransactionContextProvider>()
                .AddScoped<IndexManager>();
        }
    }
}
