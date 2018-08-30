using System;
using System.IO;
using ClosedXML.Excel;
using Kinetix.ComponentModel;
using Kinetix.Reporting.Excel;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class ExcelBuilder : IExcelBuilder
    {
        private readonly IXLWorkbook _workbook = new XLWorkbook();
        private readonly BeanDescriptor _beanDescriptor;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanDescriptor">BeanDescriptor.</param>
        /// <param name="fileName">Nom du fichier.</param>
        public ExcelBuilder(BeanDescriptor beanDescriptor, string fileName)
        {
            _beanDescriptor = beanDescriptor;
            FileName = fileName;
        }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <inheritdoc cref="IWorksheetBuilder{T}.AddWorksheet" />
        public IWorksheetBuilder<T> AddWorksheet<T>(string name)
        {
            return new WorksheetBuilder<T>(_beanDescriptor, _workbook.AddWorksheet(name), this);
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
}
