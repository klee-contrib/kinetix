using ClosedXML.Excel;
using Kinetix.Reporting.Core.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Core.Internal.Excel;

internal class ExcelBuilder : IExcelBuilder
{
    private readonly IReferenceManager _referenceManager;
    private readonly IXLWorkbook _workbook = new XLWorkbook();

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="fileName">Nom du fichier.</param>
    /// <param name="referenceManager">ReferenceManager.</param>
    public ExcelBuilder(string fileName, IReferenceManager referenceManager)
    {
        FileName = fileName;
        _referenceManager = referenceManager;
    }

    /// <inheritdoc />
    public string FileName { get; set; }

    /// <inheritdoc cref="IExcelBuilder.AddWorksheet{T}" />
    public IWorksheetBuilder<T> AddWorksheet<T>(string name)
    {
        return new WorksheetBuilder<T>(this, _referenceManager, _workbook.AddWorksheet(name));
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
    public byte[] Build(Action<IXLWorkbook> preBuildAction)
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
