using System;
using System.IO;
using ClosedXML.Excel;
using Kinetix.Reporting.Excel;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class ExcelBuilder : IExcelBuilder
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Provider injecté.</param>
        public ExcelBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <inheritdoc />
        public IXLWorkbook Workbook { get; } = new XLWorkbook();

        /// <inheritdoc />
        public IXLWorksheets Worksheets => Workbook.Worksheets;

        /// <inheritdoc cref="IWorksheetBuilder{T}.AddWorksheet" />
        public IWorksheetBuilder<T> AddWorksheet<T>(string name)
        {
            var worksheetBuilder = _provider.GetRequiredService<IWorksheetBuilder<T>>();
            worksheetBuilder.Worksheet = Workbook.AddWorksheet(name);
            worksheetBuilder.ExcelBuilder = this;
            return worksheetBuilder;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
        public byte[] Build()
        {
            byte[] b;
            using (var ms = new MemoryStream())
            using (var br = new BinaryReader(ms))
            {
                Workbook.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                b = br.ReadBytes((int)ms.Length);
            }

            return b;
        }
    }
}
