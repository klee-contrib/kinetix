using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection builder, ILogger logger, params Assembly[] serviceAssemblies)
        {
            // TODO
        }
    }
}
