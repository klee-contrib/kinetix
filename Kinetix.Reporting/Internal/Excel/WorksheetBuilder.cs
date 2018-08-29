using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Kinetix.Reporting.Excel;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class WorksheetBuilder<T> : IWorksheetBuilder<T>
    {
        /// <inheritdoc />
        public IXLWorksheet Worksheet { get; set; }

        /// <inheritdoc />
        public IExcelBuilder ExcelBuilder { get; set; }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
        public IExcelBuilder Build()
        {
            // TODO
            return ExcelBuilder;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Column(Func{T, object})" />
        public IWorksheetBuilder<T> Column(Func<T, object> selector)
        {
            return Column(string.Empty, selector);
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Column(string, Func{T, object})" />
        public IWorksheetBuilder<T> Column(string label, Func<T, object> selector)
        {
            // TODO
            return this;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Data" />
        public IWorksheetBuilder<T> Data(IEnumerable<T> data)
        {
            // TODO
            return this;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Transpose" />
        public IWorksheetBuilder<T> Transpose()
        {
            // TODO
            return this;
        }
    }
}
