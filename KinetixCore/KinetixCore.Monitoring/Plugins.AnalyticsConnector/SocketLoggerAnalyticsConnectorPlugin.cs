using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Kinetix.Monitoring.Abstractions;
using Kinetix.Monitoring.Appender;
using KinetixCore.Monitoring.Config;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KinetixCore.Monitoring
{
    public class SocketLoggerAnalyticsConnectorPlugin : IAnalyticsConnectorPlugin
    {
        // DefaultPort of SocketAppender 4650 for log4j and 4562 for log4j2
        // 4564 for log4net
        private static int DEFAULT_SERVER_PORT = 4564;

        private Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        private ILogger socketLogger;
        //private ILogger socketHealthLogger;
        //private ILogger socketMetricLogger;
        private string hostName;
        private int port;

        private string appName;
        private string localHostName;

        internal ConcurrentQueue<AProcess> ProcessQueue { get; } = new ConcurrentQueue<AProcess>();


        public SocketLoggerAnalyticsConnectorPlugin(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            IOptions<SocketLoggerAnalyticsConnectorPluginConfig> optConfig)
        {
            _loggerFactory = loggerFactory;
            SocketLoggerAnalyticsConnectorPluginConfig config = optConfig.Value;

            appName = config.AppName ?? Assembly.GetEntryAssembly().GetName().Name;
            hostName = config.HostName ?? "analytica.part.klee.lan.net";
            port = config.Port ?? DEFAULT_SERVER_PORT;
            localHostName = retrieveHostName();
        }


        public void Add(IAProcess process)
        {
            Debug.Assert(process != null);
            //---
            ProcessQueue.Enqueue((AProcess)process);
        }


        private static String retrieveHostName()
        {
            return Environment.MachineName;
        }

        private ILogger CreateLogger(string loggerName, string hostName, int port)
        {

            ILog log = LogManager.GetLogger(Assembly.GetExecutingAssembly(), loggerName);

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetExecutingAssembly());

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = PatternLayout.DefaultConversionPattern;

            TcpAppender appender = new TcpAppender()
            {
                Name = "SocketAnalytics",
                Layout = patternLayout,
                RemoteHostname = hostName,
                RemotePort = port
            };

            patternLayout.ActivateOptions();

            appender.Layout = patternLayout;
            appender.ActivateOptions();

            Logger logger = (Logger)log.Logger;
            logger.AddAppender(appender);
            logger.Level = log4net.Core.Level.Info;

            hierarchy.Root.AddAppender(appender);

            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);

            return new Log4NetLogger(log);
        }


        internal void SendProcess(AProcess process)
        {
            if (socketLogger == null)
            {
                socketLogger = CreateLogger(this.GetType().Name, hostName, port);
            }
            SendObject(process, socketLogger);
        }

        private void SendObject(AProcess obj, ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var log = new JObject();
                log.Add("AppName", appName);
                log.Add("Host", localHostName);
                log.Add("Event", JObject.FromObject(obj));

                logger.LogInformation(JsonConvert.SerializeObject(log));
            }
        }
    }
}
