using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, ServicesConfig config = null)
        {
            IEnumerable<AssemblyName> GetReferencedAssemblyNames(Assembly assembly) =>
                assembly.GetReferencedAssemblies().Where(name => name.FullName.StartsWith(config?.ServiceAssemblyPrefix ?? string.Empty));

            IEnumerable<Assembly> GetReferencedAssemblies(IEnumerable<Assembly> assemblies)
            {
                var referencedAssemblies = assemblies.SelectMany(GetReferencedAssemblyNames).Select(Assembly.Load);

                if (!referencedAssemblies.Any())
                {
                    return new List<Assembly>();
                }

                return assemblies.Concat(GetReferencedAssemblies(referencedAssemblies)).Distinct();
            }

            var contractTypes = new List<Type>();

            foreach (var type in (config?.ServiceAssemblies ?? new List<Assembly>()).Concat(GetReferencedAssemblies(new[] { Assembly.GetEntryAssembly() })).SelectMany(x => x.GetExportedTypes()))
            {
                var registerImplAttribute = type.GetCustomAttribute<RegisterImplAttribute>();
                if (registerImplAttribute != null)
                {
                    var hasContract = false;
                    foreach (var interfaceType in type.GetInterfaces())
                    {
                        if (interfaceType.GetCustomAttribute<RegisterContractAttribute>() != null)
                        {
                            contractTypes.Add(interfaceType);
                            switch (registerImplAttribute.Lifetime)
                            {
                                case ServiceLifetime.Scoped:
                                    services.AddScoped(interfaceType, type);
                                    break;
                                case ServiceLifetime.Singleton:
                                    services.AddSingleton(interfaceType, type);
                                    break;
                                case ServiceLifetime.Transient:
                                    services.AddTransient(interfaceType, type);
                                    break;
                            }

                            hasContract = true;
                        }
                    }

                    if (!hasContract)
                    {
                        switch (registerImplAttribute.Lifetime)
                        {
                            case ServiceLifetime.Scoped:
                                services.AddScoped(type);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(type);
                                break;
                            case ServiceLifetime.Transient:
                                services.AddTransient(type);
                                break;
                        }
                    }
                }
            }

            services.AddMemoryCache();
            services.AddScoped<IReferenceManager, ReferenceManager>(provider =>
            {
                var referenceManager = new ReferenceManager(provider, config?.StaticListCacheDuration, config?.ReferenceListCacheDuration);

                foreach (var interfaceType in contractTypes)
                {
                    referenceManager.RegisterAccessors(interfaceType);
                }

                return referenceManager;
            });

            return services;
        }
    }
}
