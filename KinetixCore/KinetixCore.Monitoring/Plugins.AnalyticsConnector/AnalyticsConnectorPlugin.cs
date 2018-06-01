using Kinetix.Monitoring.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KinetixCore.Monitoring
{
    public class LoggerAnalyticsConnectorPlugin : IAnalyticsConnectorPlugin
    {
        private ILoggerFactory _iLoggerFactory;

        public LoggerAnalyticsConnectorPlugin(ILoggerFactory iLoggerFactory)
        {
            _iLoggerFactory = iLoggerFactory;
        }

        public void Add(IAProcess process)
        {
            ILogger logger = _iLoggerFactory.CreateLogger(process.Category);
            if (logger.IsEnabled(LogLevel.Information))
            {
                string json = JsonConvert.SerializeObject(process);
                logger.LogInformation(json);
            }
        }

    }
}