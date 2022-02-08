using Kinetix.Reporting.Core.Excel;
using Kinetix.Reporting.Core.Internal.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Core.Internal;

internal class ReportBuilder : IReportBuilder
{
    private readonly IReferenceManager _referenceManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager injecté.</param>
    public ReportBuilder(IReferenceManager referenceManager)
    {
        _referenceManager = referenceManager;
    }

    /// <inheritdoc cref="IReportBuilder.CreateExcelReport" />
    public IExcelBuilder CreateExcelReport(string fileName)
    {
        return new ExcelBuilder(fileName, _referenceManager);
    }
}
