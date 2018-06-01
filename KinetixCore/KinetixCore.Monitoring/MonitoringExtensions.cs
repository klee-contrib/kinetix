using System;
using System.Diagnostics;
using Kinetix.Monitoring.Abstractions;
using KinetixCore.Monitoring.Analytics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KinetixCore.Monitoring
{
    public static class MonitoringExtensions
    {

        /// <summary>
        /// Add Main Monitoring 
        /// </summary>
        /// <param name="services">DI ServicesCollection</param>
        public static IServiceCollection AddMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IAnalyticsManager, AnalyticsManager>();
            services.AddSingleton<IProcessAnalytics, ProcessAnalytics>();
            services.AddSingleton<IProcessAnalyticsTracer, ProcessAnalyticsTracer>();
            services.AddSingleton<AnalyticsEFCommandListener>();
            services.AddSingleton<AnalyticsActionFilter>();
            services.AddSingleton<AnalyticsProxy>();
            return services;
        }

        /// <summary>
        /// Add Remote Socket connector for Monitoring
        /// </summary>
        /// <param name="services">DI ServicesCollection</param>
        public static IServiceCollection AddRemoteSocketConnectorMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, SocketLoggerAnalyticsConnectorPluginTask>();
            services.AddSingleton<IAnalyticsConnectorPlugin, SocketLoggerAnalyticsConnectorPlugin>();
            return services;
        }

        /// <summary>
        /// Add Remote Socket connector for Monitoring
        /// </summary>
        /// <param name="services">DI ServicesCollection</param>
        public static IServiceCollection AddSimpleLoggerAnalyticsConnectorMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IAnalyticsConnectorPlugin, LoggerAnalyticsConnectorPlugin>();
            return services;
        }


        /// <summary>
        /// Add Analytics Action Filter to intercept REST/MVC Calls
        /// </summary>
        /// <param name="builder">MvcOptions builder</param>
        public static void AddRestAnalyticsFilter(this MvcOptions builder)
        {
            builder.Filters.AddService<AnalyticsActionFilter>();
        }


        /// <summary>
        /// Use Entity Framework Command Listener to monotre SQL requests
        /// </summary>
        /// <param name="builder">The Application Builder</param>
        /// <param name="provider">The DI Service Provider</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAnalyticsEFCommandListener(this IApplicationBuilder builder, IServiceProvider provider)
        {
            AnalyticsEFCommandListener listener = provider.GetService<AnalyticsEFCommandListener>();
            DiagnosticListener.AllListeners.Subscribe(listener);
            return builder;
        }

        /// <summary>
        /// Add Analytics Middleware to intercept all HTTPs calls
        /// </summary>
        /// <param name="builder">The Application Builder</param>
        public static IApplicationBuilder UseUrlMonitoring(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AnalyticsMiddleware>();
            return builder;
        }

    }

}
