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
                        var registerImplAttribute = type.GetCustomAttribute<RegisterImplAttribute>(false);
                        if (registerImplAttribute != null)
                        {
                            var hasContract = false;
                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                if (interfaceType.GetCustomAttribute<RegisterContractAttribute>(false) != null)
                                {
                                    logger?.LogDebug("Enregistrement du service " + interfaceType.FullName);

                                    contractTypes.Add(interfaceType);
                                    switch (registerImplAttribute.Lifetime)
                                    {
                                        case ServiceLifetime.Scoped:
                                            services.AddTransient(interfaceType, type);
                                            break;
                                        case ServiceLifetime.Singleton:
                                            services.AddSingleton(interfaceType, type);
                                            break;
                                        case ServiceLifetime.Transient:
                                            services.AddScoped(interfaceType, type);
                                            break;
                                    }

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

                services.AddScoped<IReferenceManager, ReferenceManager>(provider =>
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
