using Kinetix.Monitoring.Abstractions;

namespace KinetixCore.Monitoring
{
    public interface IAnalyticsConnectorPlugin
    {
        /// <summary>
        /// Method to add a monitoring process.
        /// </summary>
        /// <param name="process"></param>
        void Add(IAProcess process);
    }
}
