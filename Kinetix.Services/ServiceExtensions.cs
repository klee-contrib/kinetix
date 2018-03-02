using System;
using System.Collections.Generic;
using System.Reflection;
using Kinetix.Caching;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, ILogger logger, params Assembly[] serviceAssemblies)
        {
            services.AddSingleton<CacheManager>();

            var contractTypes = new List<Type>();

            foreach (var assembly in serviceAssemblies)
            {
                foreach (var module in assembly.GetModules())
                {
                    foreach (var type in module.GetTypes())
                    {
                        if (type.GetCustomAttributes(typeof(RegisterImplAttribute), false).Length > 0)
                        {
                            var hasContract = false;
                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                if (interfaceType.GetCustomAttributes(typeof(RegisterContractAttribute), false).Length > 0)
                                {
                                    logger?.LogDebug("Enregistrement du service " + interfaceType.FullName);

                                    contractTypes.Add(interfaceType);
                                    services.AddTransient(interfaceType, type);
                                    hasContract = true;
                                }
                            }

                            if (!hasContract)
                            {
                                services.AddTransient(type);
                            }
                        }
                    }
                }

                services.AddSingleton<IReferenceManager, ReferenceManager>(provider =>
                {
                    var referenceManager = new ReferenceManager(provider);

                    foreach (var interfaceType in contractTypes)
                    {
                        referenceManager.RegisterAccessors(interfaceType);
                    }

                    return referenceManager;
                });
            }

            return services;
        }
    }
}
