using System.Threading;
using System.Threading.Tasks;

namespace KinetixCore.Monitoring
{
    public interface IScheduledTask
    {

        /// <summary>
        /// Define the period of execution of the schedulled task
        /// </summary>
        long PeriodInSeconds { get; }

        /// <summary>
        /// Main method executed periodally
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
