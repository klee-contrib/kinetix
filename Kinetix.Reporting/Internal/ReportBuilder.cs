using Kinetix.ComponentModel;
using Kinetix.Reporting.Excel;
using Kinetix.Reporting.Internal.Excel;

namespace Kinetix.Reporting.Internal
{
    internal class ReportBuilder : IReportBuilder
    {
        private readonly BeanDescriptor _beanDescriptor;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanDescriptor">BeanDescriptor injecté.</param>
        public ReportBuilder(BeanDescriptor beanDescriptor)
        {
            _beanDescriptor = beanDescriptor;
        }

        /// <inheritdoc cref="IReportBuilder.CreateExcelReport" />
        public IExcelBuilder CreateExcelReport(string fileName)
        {
            return new ExcelBuilder(_beanDescriptor, fileName);
        }
    }
}
