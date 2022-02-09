using System.Linq;
using Kinetix.Edm.SharePoint;
using Kinetix.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Edm
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddEdm(this IServiceCollection services, params EdmSettings[] edmSettings)
        {
            return services
                .AddSingleton(new SharePointManager(edmSettings))
                .AddScoped<EdmAnalytics>()
                .AddScoped<IAnalytics, EdmAnalytics>()
                .AddScoped(p => new EdmManager(
                    p.GetService<SharePointManager>(),
                    p.GetService<EdmAnalytics>(),
                    p.GetService<ILogger<SharePointStore>>(),
                    edmSettings.Select(s => s.Name).ToArray()));
        }
    }
}
