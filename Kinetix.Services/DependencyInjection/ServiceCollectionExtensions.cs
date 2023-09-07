using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kinetix.Services.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static readonly ProxyGenerator _proxyGenerator = new();

    public static IServiceCollection AddInterceptedTransient<T, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> serviceFactory,
        Action<InterceptionOptions> configurator)
        where T : class
        where TImplementation : class, T
    {
        return services.AddIntercepted(
            typeof(T),
            typeof(TImplementation),
            lifetime => new ServiceDescriptor(typeof(TImplementation), serviceFactory, lifetime),
            configurator,
            ServiceLifetime.Transient);
    }

    public static IServiceCollection AddInterceptedTransient<T, TImplementation>(
        this IServiceCollection services, Action<InterceptionOptions> configurator)
        where T : class
        where TImplementation : class, T
    {
        return AddInterceptedTransient(services, typeof(T), typeof(TImplementation), configurator);
    }

    public static IServiceCollection AddInterceptedTransient(this IServiceCollection services, Type contractType, Type implType, Action<InterceptionOptions> configurator)
    {
        return services.AddIntercepted(
            contractType,
            implType,
            lifetime => ServiceDescriptor.Describe(implType, implType, lifetime),
            configurator,
            ServiceLifetime.Transient);
    }

    public static IServiceCollection AddInterceptedScoped<T, TImplementation>(this IServiceCollection services,
        Func<IServiceProvider, TImplementation> serviceFactory,
        Action<InterceptionOptions> configurator)
        where T : class where TImplementation : class, T
    {
        return services.AddIntercepted(
            typeof(T),
            typeof(TImplementation),
            lifetime => new ServiceDescriptor(typeof(TImplementation), serviceFactory, lifetime),
            configurator,
            ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddInterceptedScoped<T, TImplementation>(
        this IServiceCollection services, Action<InterceptionOptions> configurator)
        where T : class
        where TImplementation : class, T
    {
        return AddInterceptedScoped(services, typeof(T), typeof(TImplementation), configurator);
    }

    public static IServiceCollection AddInterceptedScoped(this IServiceCollection services, Type contractType, Type implType, Action<InterceptionOptions> configurator)
    {
        return services.AddIntercepted(
            contractType,
            implType,
            lifetime => ServiceDescriptor.Describe(implType, implType, lifetime),
            configurator,
            ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddInterceptedSingleton<T, TImplementation>(
        this IServiceCollection services, Action<InterceptionOptions> configurator)
        where T : class
        where TImplementation : class, T
    {
        return AddInterceptedSingleton(services, typeof(T), typeof(TImplementation), configurator);
    }

    public static IServiceCollection AddInterceptedSingleton(this IServiceCollection services, Type contractType, Type implType, Action<InterceptionOptions> configurator)
    {
        return services.AddIntercepted(
            contractType,
            implType,
            lifetime => ServiceDescriptor.Describe(implType, implType, lifetime),
            configurator,
            ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddInterceptedSingleton<T, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> serviceFactory,
        Action<InterceptionOptions> configurator)
        where T : class where TImplementation : class, T
    {
        return services.AddIntercepted(
            typeof(T),
            typeof(TImplementation),
            lifetime => new ServiceDescriptor(typeof(TImplementation), serviceFactory, lifetime),
            configurator,
            ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddIntercepted(
        this IServiceCollection services,
        Type contractType,
        Type implType,
        Func<ServiceLifetime, ServiceDescriptor> descriptorFactory,
        Action<InterceptionOptions> configurator,
        ServiceLifetime lifetime)
    {
        var interceptionOptions = new InterceptionOptions();
        configurator(interceptionOptions);

        interceptionOptions.Interceptors.ForEach(services.TryAddScoped);
        services.TryAdd(descriptorFactory(lifetime));

        services.Add(ServiceDescriptor.Describe(contractType,
            sp =>
            {
                var interceptorInstances = interceptionOptions.Interceptors
                    .Select(sp.GetRequiredService)
                    .Cast<IInterceptor>()
                    .ToArray();

                return _proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        contractType,
                        sp.GetRequiredService(implType),
                        ProxyGenerationOptions.Default,
                        interceptorInstances);
            },
            lifetime));

        return services;
    }

    public static IServiceCollection TryAddIntercepted(
        this IServiceCollection services,
        Type contractType,
        Type implType,
        Func<ServiceLifetime, ServiceDescriptor> descriptorFactory,
        Action<InterceptionOptions> configurator,
        ServiceLifetime lifetime)
    {
        var interceptionOptions = new InterceptionOptions();
        configurator(interceptionOptions);

        interceptionOptions.Interceptors.ForEach(services.TryAddScoped);
        services.TryAdd(descriptorFactory(lifetime));

        services.TryAdd(ServiceDescriptor.Describe(contractType,
            sp =>
            {
                var interceptorInstances = interceptionOptions.Interceptors
                    .Select(sp.GetRequiredService)
                    .Cast<IInterceptor>()
                    .ToArray();

                return _proxyGenerator
                    .CreateInterfaceProxyWithTarget(
                        contractType,
                        sp.GetRequiredService(implType),
                        ProxyGenerationOptions.Default,
                        interceptorInstances);
            },
            lifetime));

        return services;
    }
}
