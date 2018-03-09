using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;

namespace KinetixCore.Monitoring
{
    public static class MonitoringExtensions 
    {

        public static void AddMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IAnalyticsManager, AnalyticsManager>();
            services.AddSingleton<IProcessAnalytics, ProcessAnalytics>();
            services.AddSingleton<IProcessAnalyticsTracer, ProcessAnalyticsTracer>();
            services.AddSingleton<AnalyticsEFCommandListener>();
            services.AddSingleton<AnalyticsActionFilter>(); 
        }

        public static void AddRemoteSocketConnectorMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, SocketLoggerAnalyticsConnectorPluginTask>();
            //services.AddSingleton<SocketLoggerAnalyticsConnectorPlugin>();

            services.AddSingleton<IAnalyticsConnectorPlugin, SocketLoggerAnalyticsConnectorPlugin>();
        }

        public static void AddSimpleLoggerAnalyticsConnectorMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<IAnalyticsConnectorPlugin, LoggerAnalyticsConnectorPlugin>();
        }

        public static void UseMonitoring(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AnalyticsMiddleware>();
        }

        public static void AddAnalyticsCommandListener(this IApplicationBuilder builder, DiagnosticListener diagnosticListener, IServiceProvider provider)
        {
            // TODO : Remove this method
            AnalyticsEFCommandListener listener = provider.GetService<AnalyticsEFCommandListener>();
            diagnosticListener.SubscribeWithAdapter(listener);
            diagnosticListener.Subscribe(listener);
        }

        public static void AddAnalytics(this MvcOptions builder)
        {
            builder.Filters.AddService<AnalyticsActionFilter>();
        }

        public static T AddAnalyticsEFCommandListener<T>(this T options, IServiceProvider provider)
        {
            AnalyticsEFCommandListener listener = provider.GetService<AnalyticsEFCommandListener>();
            DiagnosticListener.AllListeners.Subscribe(listener);
            return options;
        }
    }
    
}
