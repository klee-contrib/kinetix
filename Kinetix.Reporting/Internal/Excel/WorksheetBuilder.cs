using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ClosedXML.Excel;
using Kinetix.ComponentModel;
using Kinetix.Reporting.Excel;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class WorksheetBuilder<T> : IWorksheetBuilder<T>
    {
        private readonly IList<(string label, Expression<Func<T, object>> selector)> _columns = new List<(string label, Expression<Func<T, object>> selector)>();
        private bool _transpose = false;
        private IEnumerable<T> _data;
        private readonly BeanDescriptor _beanDescriptor;
        private readonly IXLWorksheet _worksheet;
        private readonly IExcelBuilder _excelBuilder;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanDescriptor">Bean descriptor.</param>
        /// <param name="worksheet">Worksheet.</param>
        /// <param name="excelBuilder">ExcelBuilder.</param>
        public WorksheetBuilder(BeanDescriptor beanDescriptor, IXLWorksheet worksheet, ExcelBuilder excelBuilder)
        {
            _beanDescriptor = beanDescriptor;
            _worksheet = worksheet;
            _excelBuilder = excelBuilder;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
        public IExcelBuilder Build(Action<IXLWorksheet> postBuildAction)
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                var (label, selector) = _columns[i];
                if (_transpose)
                {
                    _worksheet.Cell(i + 1, 1).Value = label;
                }
                else
                {
                    _worksheet.Cell(1, i + 1).Value = label;
                }

                var definition = _beanDescriptor.GetDefinition(typeof(T));

                BeanPropertyDescriptor property = null;
                if (selector.Body is UnaryExpression u && u.Operand is MemberExpression me)
                {
                    property = definition.Properties.Single(p => p.PropertyName == me.Member.Name);
                }

                var j = 1;
                foreach (var item in _data)
                {
                    j++;
                    var cell = _transpose
                        ? _worksheet.Cell(i + 1, j)
                        : _worksheet.Cell(j, i + 1);

                    cell.SetValue(selector.Compile()(item));

                    if (property != null)
                    {
                        cell.Style.NumberFormat.Format = property.Domain.FormatString;
                    }
                }
            }

            _worksheet.ColumnsUsed().AdjustToContents();
            postBuildAction?.Invoke(_worksheet);
            return _excelBuilder;
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Column(Func{T, object})" />
        public IWorksheetBuilder<T> Column(Expression<Func<T, object>> selector)
        {
            return Column(string.Empty, selector);
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Column(string)" />
        public IWorksheetBuilder<T> Column(string label = null)
        {
            return Column(label, _ => null);
        }

        /// <inheritdoc cref="IWorksheetBuilder{T}.Column(string, Func{T, object})" />
        public IWorksheetBuilder<T> Column(string label, Expression<Func<T, object>> selector)
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
