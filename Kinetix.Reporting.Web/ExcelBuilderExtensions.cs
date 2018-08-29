using System;
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
        /// <returns>FileContentResult.</returns>
        public static FileContentResult BuildFileContentResult(this IExcelBuilder builder)
        {
            return new FileContentResult(builder.Build(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = builder.FileName,
                LastModified = DateTime.Now
            };
        }
    }
}
