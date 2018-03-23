using System;
using System.Collections.Generic;
using System.Reflection;
using Kinetix.Caching;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KinetixCore.Monitoring;
using KinetixCore.Monitoring.Analytics;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Kinetix.Services;

namespace Kinetix.Services
{
    public static class ServiceExtensions
    {
        public static void AddServices(this ContainerBuilder builder, ILogger logger, params Assembly[] serviceAssemblies)
        {
            builder.RegisterType<CacheManager>();

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

                            // Analytics attributes will be looked on RegisterImpl Class only.
                            var proxyAnalytics = false;
                            if (type.GetCustomAttributes(typeof(AnalyticsAttribute), false).Length > 0)
                            {
                                proxyAnalytics = true;
                            }
                            else
                            {
                                foreach (var method in type.GetMethods())
                                {
                                    if (method.GetCustomAttributes(typeof(AnalyticsAttribute), false).Length > 0)
                                    {
                                        proxyAnalytics = true;
                                        break;
                                    }
                                }
                            }

                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                if (interfaceType.GetCustomAttributes(typeof(RegisterContractAttribute), false).Length > 0)
                                {
                                    logger?.LogDebug("Enregistrement du service " + interfaceType.FullName);

                                    contractTypes.Add(interfaceType);
                                    if (proxyAnalytics)
                                    {
                                        builder.RegisterType(type)
                                               .As(interfaceType)
                                               .EnableInterfaceInterceptors()
                                               .InterceptedBy(typeof(AnalyticsProxy));
                                    }
                                    else
                                    {
                                        builder.RegisterType(type).As(interfaceType);
                                    }
                                    hasContract = true;
                                }
                            }

                            if (!hasContract)
                            {
                                builder.RegisterType(type);
                            }
                        }
                    }
                }

                builder.Register(cc =>
                {
                    var referenceManager = new ReferenceManager(cc);

                    foreach (var interfaceType in contractTypes)
                    {
                        referenceManager.RegisterAccessors(interfaceType);
                    }

                    return referenceManager;
                }).As<IReferenceManager>();

            }

        }
    }
}
