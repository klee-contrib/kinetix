using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Database
{
    /// <summary>
    /// Méthodes d'extensions.
    /// </summary>
    public static class ServiceExtensions
    {
        public static void AddDatabase(this IServiceCollection services, Assembly constDataTypes = null, ResourceManager constraintMesssages = null, ResourceManager includeQueries = null)
        {
            services.AddSingleton<TransactionalContext>();
            services.AddSingleton(provider =>
            {
                var manager = new SqlServerManager(
                    provider.GetService<ILogger<SqlServerManager>>(),
                    provider.GetService<TransactionalContext>(),
                    provider.GetService<IConfiguration>());

                if (constDataTypes != null)
                {
                    manager.RegisterConstDataTypes(constDataTypes);
                }

                if (constraintMesssages != null)
                {
                    manager.RegisterConstraintMessageResource(constraintMesssages);
                }

                if (includeQueries != null)
                {
                    manager.RegisterIncludeQueryResource(includeQueries);
                }

                return manager;
            });
        }
    }
}
