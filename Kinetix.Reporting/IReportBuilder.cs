using Kinetix.Reporting.Excel;

namespace Kinetix.Reporting
{
    /// <summary>
    /// Permet de construire des rapports.
    /// </summary>
    public interface IReportBuilder
    {
        /// <summary>
        /// Crée un rapport Excel.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        /// <returns></returns>
        IExcelBuilder CreateExcelReport(string fileName);
    }
}
