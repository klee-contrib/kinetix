using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using Kinetix.Caching;
using Kinetix.Services.Annotations;
using KinetixCore.Monitoring.Analytics;
using KinetixCore.SqlServer;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.Autofac
{
    public static class ServiceExtensions
    {
        public static void AddServices(this ContainerBuilder builder, ILogger logger, params Assembly[] serviceAssemblies)
        {
            builder.RegisterType<CacheManager>();
            builder.RegisterType<TransactionalProxy>();

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

                            // Analytics and Transactional attributes will be looked on RegisterImpl Class only.
                            bool proxyAnalytics = HasMethodOrClassAttribute(type, typeof(AnalyticsAttribute));
                            bool proxyTransactional = HasMethodOrClassAttribute(type, typeof(TransactionalAttribute));

                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                if (interfaceType.GetCustomAttributes(typeof(RegisterContractAttribute), false).Length > 0)
                                {
                                    logger?.LogDebug("Enregistrement du service " + interfaceType.FullName);

                                    contractTypes.Add(interfaceType);

                                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb = builder
                                                   .RegisterType(type)
                                                   .As(interfaceType);

                                    if (proxyAnalytics || proxyTransactional)
                                    {
                                        rb.EnableInterfaceInterceptors();
                                        if (proxyAnalytics)
                                        {
                                            rb.InterceptedBy(typeof(AnalyticsProxy));
                                        }
                                        if (proxyTransactional)
                                        {
                                            rb.InterceptedBy(typeof(TransactionalProxy));
                                        }
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
                    var referenceManager = new ReferenceManager(cc.Resolve<IServiceProvider>());

                    foreach (var interfaceType in contractTypes)
                    {
                        referenceManager.RegisterAccessors(interfaceType);
                    }

                    return referenceManager;
                }).As<IReferenceManager>();
            }
        }

        private static bool HasMethodOrClassAttribute(Type type, Type attribute)
        {
            var attributeFound = false;
            if (type.GetCustomAttributes(attribute, false).Length > 0)
            {
                attributeFound = true;
            }
            else
            {
                foreach (var method in type.GetMethods())
                {
                    if (method.GetCustomAttributes(attribute, false).Length > 0)
                    {
                        attributeFound = true;
                        break;
                    }
                }
            }

            return attributeFound;
        }
    }
}