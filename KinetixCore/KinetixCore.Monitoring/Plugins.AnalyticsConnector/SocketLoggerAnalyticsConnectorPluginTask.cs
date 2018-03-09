using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KinetixCore.Monitoring
{
    public class SocketLoggerAnalyticsConnectorPluginTask : ScheduledService
    {

        private SocketLoggerAnalyticsConnectorPlugin connectorPlugin;

        //public SocketLoggerAnalyticsConnectorPluginTask(SocketLoggerAnalyticsConnectorPlugin socketConnectorPlugin)
        public SocketLoggerAnalyticsConnectorPluginTask(IAnalyticsConnectorPlugin socketConnectorPlugin)
        {
            connectorPlugin = (SocketLoggerAnalyticsConnectorPlugin) socketConnectorPlugin;
        }

        public long PeriodInSeconds => 1;

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (!connectorPlugin.ProcessQueue.IsEmpty)
                {
                    AProcess head;
                    bool success = connectorPlugin.ProcessQueue.TryDequeue(out head);
                    if (success && head != null)
                    {
                        connectorPlugin.SendProcess(head);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(PeriodInSeconds), cancellationToken);
            }
        }

    }
}
