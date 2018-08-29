using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Kinetix.Reporting.Excel;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class WorksheetBuilder<T> : IWorksheetBuilder<T>
    {
        private readonly IList<(string label, Func<T, object> selector)> _columns = new List<(string label, Func<T, object> selector)>();
        private IEnumerable<T> _data;
        private bool _transpose = false;

        /// <inheritdoc />
        public IXLWorksheet Worksheet { get; set; }

        /// <inheritdoc />
        public IExcelBuilder ExcelBuilder { get; set; }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
        public IExcelBuilder Build()
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                var (label, selector) = _columns[i];
                if (_transpose)
                {
                    Worksheet.Cell(i + 1, 1).Value = label;
                }
                else
                {
                    Worksheet.Cell(1, i + 1).Value = label;
                }

                var j = 1;
                foreach (var item in _data)
                {
                    j++;
                    if (_transpose)
                    {
                        Worksheet.Cell(i + 1, j).SetValue(selector(item));
                    }
                    else
                    {
                        Worksheet.Cell(j, i + 1).SetValue(selector(item));
                    }
                }
            }

            Worksheet.ColumnsUsed().AdjustToContents();
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
            _columns.Add((label, selector));
            return this;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Data" />
        public IWorksheetBuilder<T> Data(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Transpose" />
        public IWorksheetBuilder<T> Transpose()
        {
            _transpose = true;
            return this;
        }
    }
}
