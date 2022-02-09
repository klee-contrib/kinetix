using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kinetix.Monitoring;
using Kinetix.Services.Annotations;
using Kinetix.Services.DependencyInjection;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services
{
    public static class ServiceExtensions
    {
        private static readonly Action<InterceptionOptions> defaultIOptions = i => i
            .With<AnalyticsInterceptor>()
            .With<TransactionInterceptor>();

        /// <summary>
        /// Enregistre les services Kinetix.
        /// </summary>
        /// <param name="services">ServiceCollection.</param>
        /// <param name="serviceAssemblyPrefix">Préfixe des assemblies de services à enregistrer.</param>
        /// <param name="builder">Configurateur.</param>
        /// <returns>ServiceCollection.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services, string serviceAssemblyPrefix, Action<ServicesConfig> builder)
        {
            var config = new ServicesConfig { ServiceAssemblyPrefix = serviceAssemblyPrefix };
            builder(config);

            IEnumerable<AssemblyName> GetReferencedAssemblyNames(Assembly assembly) =>
                assembly.GetReferencedAssemblies().Where(name => name.FullName.StartsWith(config.ServiceAssemblyPrefix));

            IEnumerable<Assembly> GetReferencedAssemblies(IEnumerable<Assembly> assemblies)
            {
                var referencedAssemblies = assemblies.SelectMany(GetReferencedAssemblyNames).Select(Assembly.Load);

                return !referencedAssemblies.Any()
                    ? new List<Assembly>()
                    : assemblies.Concat(GetReferencedAssemblies(referencedAssemblies)).Distinct();
            }

            var contractTypes = new List<Type>();

            var assemblies = GetReferencedAssemblies(
                config.ServiceAssemblies
                    .Concat(new[] { Assembly.GetEntryAssembly() })
                    .Where(a => a != null))
                .Distinct();

            foreach (var type in assemblies.SelectMany(x => x.GetExportedTypes()))
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
                            var iOptions = config.InterceptionOptions != null ? config.InterceptionOptions(interfaceType) : defaultIOptions;
                            switch (registerImplAttribute.Lifetime)
                            {
                                case ServiceLifetime.Scoped:
                                    if (iOptions != null)
                                    {
                                        services.AddInterceptedScoped(interfaceType, type, iOptions);
                                    }
                                    else
                                    {
                                        services.AddScoped(interfaceType, type);
                                    }

                                    break;
                                case ServiceLifetime.Singleton:
                                    if (iOptions != null)
                                    {
                                        services.AddInterceptedSingleton(interfaceType, type, iOptions);
                                    }
                                    else
                                    {
                                        services.AddSingleton(interfaceType, type);
                                    }

                                    break;
                                case ServiceLifetime.Transient:
                                    if (iOptions != null)
                                    {
                                        services.AddInterceptedTransient(interfaceType, type, iOptions);
                                    }
                                    else
                                    {
                                        services.AddTransient(interfaceType, type);
                                    }

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

            services
                .AddMemoryCache()
                .AddScoped<ServicesAnalytics>()
                .AddScoped<ServiceScopeManager>()
                .AddScoped<IAnalytics, ServicesAnalytics>(p => p.GetService<ServicesAnalytics>())
                .AddScoped<IReferenceManager>(provider =>
                {
                    var referenceManager = new ReferenceManager(provider, config.StaticListCacheDuration, config.ReferenceListCacheDuration);

                    foreach (var interfaceType in contractTypes)
                    {
                        referenceManager.RegisterAccessors(interfaceType);
                    }

                    return referenceManager;
                })
                .AddScoped<IFileManager>(provider =>
                {
                    var fileManager = new FileManager(provider);

                    foreach (var interfaceType in contractTypes)
                    {
                        fileManager.RegisterAccessors(interfaceType);
                    }

                    return fileManager;
                });

            return services;
        }
    }
}
