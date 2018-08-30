using Kinetix.Reporting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Reporting
{
    public static class ServicesExtensions
    {
        /// <summary>
        /// Ajoute les services pourt le reporting.
        /// </summary>
        /// <param name="services">ServiceCollection.</param>
        /// <returns>ServiceCollection.</returns>
        public static IServiceCollection AddReporting(this IServiceCollection services)
        {
            return services.AddSingleton<IReportBuilder, ReportBuilder>();
        }
    }
}
