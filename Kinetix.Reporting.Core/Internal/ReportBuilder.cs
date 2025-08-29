using Kinetix.Reporting.Core.Excel;
using Kinetix.Reporting.Core.Internal.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Core.Internal;

/// <summary>
/// Constructeur.
/// </summary>
/// <param name="referenceManager">ReferenceManager injecté.</param>
internal class ReportBuilder(IReferenceManager referenceManager) : IReportBuilder
{
    /// <inheritdoc cref="IReportBuilder.CreateExcelReport" />
    public IExcelBuilder CreateExcelReport(string fileName)
    {
        return new ExcelBuilder(fileName, referenceManager);
    }
}
