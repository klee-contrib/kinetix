using ClosedXML.Excel;

namespace Kinetix.Reporting.Excel
{
    /// <summary>
    /// Constructeur d'un rapport Excel (Workbook).
    /// </summary>
    public interface IExcelBuilder
    {
        /// <summary>
        /// Nom du fichier Excel.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Workbook ClosedXML, pour manipuler directement le fichier Excel en cours de construction.
        /// </summary>
        IXLWorkbook Workbook { get; }

        /// <summary>
        /// Ajoute une Worksheet au Workbook Excel avec son constructeur.
        /// </summary>
        /// <typeparam name="T">Type de l'objet représenté dans la Worksheet.</typeparam>
        /// <param name="name">Nom de la worksheet.</param>
        /// <returns>Le constructeur de Worksheet.</returns>
        IWorksheetBuilder<T> AddWorksheet<T>(string name);

        /// <summary>
        /// Crée le fichier binaire à partir du Workbook précédemment créé.
        /// </summary>
        /// <returns>Le fichier binaire.</returns>
        byte[] Build();
    }
}