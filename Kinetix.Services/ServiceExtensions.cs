using System.Reflection;
using Kinetix.Services.Annotations;
using Kinetix.Services.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kinetix.Services;

public static class ServiceExtensions
{
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
                        var iOptions = config.InterceptionOptions != null ? config.InterceptionOptions(interfaceType) : null;

                        if (iOptions != null)
                        {
                            services.TryAddIntercepted(
                                interfaceType,
                                type,
                                lifetime => ServiceDescriptor.Describe(type, type, lifetime),
                                iOptions,
                                registerImplAttribute.Lifetime);
                        }
                        else
                        {
                            services.TryAdd(new ServiceDescriptor(interfaceType, type, registerImplAttribute.Lifetime));
                        }

                        hasContract = true;
                    }
                }

                if (!hasContract)
                {
                    services.TryAdd(new ServiceDescriptor(type, type, registerImplAttribute.Lifetime));
                }
            }
        }

        services.AddMemoryCache();
        services.TryAddScoped<TransactionScopeManager>();
        services.TryAddScoped<IReferenceManager>(provider =>
        {
            var referenceManager = new ReferenceManager(provider, config.StaticListCacheDuration, config.ReferenceListCacheDuration);

            foreach (var interfaceType in services.Select(s => s.ServiceType).Where(s => s.GetCustomAttribute<RegisterContractAttribute>() != null))
            {
                referenceManager.RegisterAccessors(interfaceType);
            }

            return referenceManager;
        });
        services.TryAddScoped<IFileManager>(provider =>
        {
            var fileManager = new FileManager(provider);

            foreach (var interfaceType in services.Select(s => s.ServiceType).Where(s => s.GetCustomAttribute<RegisterContractAttribute>() != null))
            {
                fileManager.RegisterAccessors(interfaceType);
            }

            return fileManager;
        });

        if (config.ReferenceNotifier != null)
        {
            services.AddSingleton(typeof(IReferenceNotifier), config.ReferenceNotifier);
        }

        return services;
    }
}
