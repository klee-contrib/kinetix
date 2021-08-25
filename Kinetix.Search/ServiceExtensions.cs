using Kinetix.Search.MetaModel;
using Kinetix.Services.DependencyInjection.Interceptors;
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
                .AddScoped<IndexManager>()
                .AddScoped<IOnBeforeCommit, FlushOnBeforeCommit>();
        }
    }

    public class FlushOnBeforeCommit : IOnBeforeCommit
    {
        private readonly IndexManager _indexManager;

        public FlushOnBeforeCommit(IndexManager indexManager)
        {
            _indexManager = indexManager;
        }

        public bool DisableFlush { get; set; }

        public void OnBeforeCommit()
        {
            if (!DisableFlush)
            {
                _indexManager.Flush();
            }
        }
    }
}
