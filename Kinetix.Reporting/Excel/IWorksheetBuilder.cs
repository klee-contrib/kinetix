using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace Kinetix.Reporting.Excel
{
    /// <summary>
    /// Constructeur d'onglet Excel (Worksheet).
    /// </summary>
    /// <typeparam name="T">Type de l'objet représenté dans la Worksheet.</typeparam>
    public interface IWorksheetBuilder<T>
    {
        /// <summary>
        /// ExcelBuilder parent.
        /// </summary>
        IExcelBuilder ExcelBuilder { get; set; }

        /// <summary>
        /// Worksheet ClosedXML, pour manipuler directement l'onglet Excel en cours de construction.
        /// </summary>
        IXLWorksheet Worksheet { get; set; }

        /// <summary>
        /// Configure une nouvelle colonne dans la Worksheet.
        /// </summary>
        /// <param name="selector">Le champ à afficher dans la colonne.</param>
        /// <returns>Builder.</returns>
        IWorksheetBuilder<T> Column(Func<T, object> selector);

        /// <summary>
        /// Configure une nouvelle colonne dans la Worksheet.
        /// </summary>
        /// <param name="label">Le libellé de la colonne.</param>
        /// <returns>Builder.</returns>
        IWorksheetBuilder<T> Column(string label = null);

        /// <summary>
        /// Configure une nouvelle colonne dans la Worksheet.
        /// </summary>
        /// <param name="label">Le libellé de la colonne.</param>
        /// <param name="selector">Le champ à afficher dans la colonne.</param>
        /// <returns>Builder.</returns>
        IWorksheetBuilder<T> Column(string label, Func<T, object> selector);

        /// <summary>
        /// Configure la source de données pour la Worksheet.
        /// </summary>
        /// <param name="data">Les données.</param>
        /// <returns>Builder.</returns>
        IWorksheetBuilder<T> Data(IEnumerable<T> data);

        /// <summary>
        /// Transpose la feuille pour échanger les colonnes et les lignes.
        /// </summary>
        /// <returns>Builder.</returns>
        IWorksheetBuilder<T> Transpose();

        /// <summary>
        /// Crée la Worksheet à partir de ce qui a été configuré.
        /// </summary>
        /// <returns>ExcelBuilder.</returns>
        IExcelBuilder Build();
    }
}