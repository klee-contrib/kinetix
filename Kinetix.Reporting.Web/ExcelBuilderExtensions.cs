using System;
using ClosedXML.Excel;
using Kinetix.Reporting.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Reporting.Web
{
    public static class ExcelBuilderExtensions
    {
        /// <summary>
        /// Construit le fichier Excel en tant que FileContentResult pour MVC.
        /// </summary>
        /// <param name="builder">ExcelBuilder.</param>
        /// <param name="preBuildAction">Actions manuelles à effectuer sur le workbook avant finalisation.</param>
        /// <returns>FileContentResult.</returns>
        public static FileContentResult BuildFileContentResult(this IExcelBuilder builder, Action<IXLWorkbook> preBuildAction = null)
        {
            return new FileContentResult(builder.Build(preBuildAction), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = builder.FileName,
                LastModified = DateTime.Now
            };
        }
    }
}
