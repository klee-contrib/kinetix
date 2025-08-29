using ClosedXML.Excel;
using Kinetix.Reporting.Core.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Core.Internal.Excel;

/// <summary>
/// Constructeur.
/// </summary>
/// <param name="fileName">Nom du fichier.</param>
/// <param name="referenceManager">ReferenceManager.</param>
internal class ExcelBuilder(string fileName, IReferenceManager referenceManager) : IExcelBuilder
{
    private readonly XLWorkbook _workbook = new();

    /// <inheritdoc />
    public string FileName { get; set; } = fileName;

    /// <inheritdoc cref="IExcelBuilder.AddWorksheet{T}" />
    public IWorksheetBuilder<T> AddWorksheet<T>(string name)
    {
        return new WorksheetBuilder<T>(this, referenceManager, _workbook.AddWorksheet(name));
    }

    /// <inheritdoc cref="IExcelBuilder.Build" />
    public byte[] Build(Action<IXLWorkbook> preBuildAction = null)
    {
        preBuildAction?.Invoke(_workbook);

        byte[] b;
        using (var ms = new MemoryStream())
        using (var br = new BinaryReader(ms))
        {
            _workbook.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);
            b = br.ReadBytes((int)ms.Length);
        }

        return b;
    }
}
