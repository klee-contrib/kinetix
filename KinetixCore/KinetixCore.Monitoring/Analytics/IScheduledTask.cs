using System.Threading;
using System.Threading.Tasks;

namespace KinetixCore.Monitoring
{
    public interface IScheduledTask
    {

        long PeriodInSeconds { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
