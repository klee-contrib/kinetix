using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Logging.Log4Net
{
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private string _configFileName = string.Empty;
        private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        public Log4NetLoggerProvider(string configFileName)
        {
            _configFileName = configFileName;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private Log4NetLogger CreateLoggerImplementation(string categoryName)
        {
            var repository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());

            if (log4net.LogManager.GetCurrentLoggers(repository.Name).Count() == 0)
            {
                log4net.Config.XmlConfigurator.Configure(repository, new FileInfo(_configFileName));
            }

            var logger = log4net.LogManager.GetLogger(repository.Name, categoryName);
            return new Log4NetLogger(logger);
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}