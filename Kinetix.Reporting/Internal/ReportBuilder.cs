using System;
using Kinetix.Reporting.Excel;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Reporting.Internal
{
    internal class ReportBuilder : IReportBuilder
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Provider injecté.</param>
        public ReportBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc cref="IReportBuilder.CreateExcelReport" />
        public IExcelBuilder CreateExcelReport(string fileName)
        {
            var excelBuilder = _provider.GetRequiredService<IExcelBuilder>();
            excelBuilder.FileName = fileName;
            return excelBuilder;
        }
    }
}
