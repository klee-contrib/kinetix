using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ClosedXML.Excel;
using Kinetix.ComponentModel;
using Kinetix.Reporting.Annotations;
using Kinetix.Reporting.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Internal.Excel
{
    internal class WorksheetBuilder<T> : IWorksheetBuilder<T>
    {
        private readonly IList<(string label, Expression<Func<T, object>> selector)> _columns = new List<(string label, Expression<Func<T, object>> selector)>();
        private readonly IExcelBuilder _excelBuilder;
        private readonly IReferenceManager _referenceManager;
        private readonly IXLWorksheet _worksheet;

        private IEnumerable<T> _data;
        private bool _transpose = false;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="referenceManager">ReferenceManager.</param>
        /// <param name="excelBuilder">ExcelBuilder.</param>
        /// <param name="worksheet">Worksheet.</param>
        public WorksheetBuilder(ExcelBuilder excelBuilder, IReferenceManager referenceManager, IXLWorksheet worksheet)
        {
            _excelBuilder = excelBuilder;
            _referenceManager = referenceManager;
            _worksheet = worksheet;
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

                var definition = BeanDescriptor.GetDefinition(typeof(T));
                (string True, string False) booleanFormat = default;
                string dateFormat = null;
                string numberFormat = null;
                Type referenceType = null;

                if (selector.Body is not MemberExpression me)
                {
                    me = (selector.Body as UnaryExpression)?.Operand as MemberExpression;
                }

                if (me != null)
                {
                    var def = definition;

                    if (me.Expression is MethodCallExpression mce)
                    {
                        def = BeanDescriptor.GetDefinition(mce.Type);
                    }

                    var property = def.Properties.SingleOrDefault(p => p.PropertyName == me.Member.Name);
                    booleanFormat = property?.Domain.ExtraAttributes.OfType<BooleanFormatAttribute>().SingleOrDefault()?.Format ?? default;
                    dateFormat = property?.Domain.ExtraAttributes.OfType<DateFormatAttribute>().SingleOrDefault()?.Format;
                    numberFormat = property?.Domain.ExtraAttributes.OfType<NumberFormatAttribute>().SingleOrDefault()?.Format;
                    referenceType = property?.ReferenceType;
                }

                var j = 1;
                foreach (var item in _data)
                {
                    j++;
                    var cell = _transpose
                        ? _worksheet.Cell(i + 1, j)
                        : _worksheet.Cell(j, i + 1);

                    var value = selector.Compile()(item);

                    if (referenceType != null)
                    {
                        cell.SetValue(_referenceManager.GetReferenceValue(referenceType, value));
                    }
                    else if (booleanFormat != default && value is bool b)
                    {
                        cell.SetValue(b ? booleanFormat.True : booleanFormat.False);
                    }
                    else
                    {
                        cell.SetValue(value);
                    }

                    if (dateFormat != null)
                    {
                        cell.Style.DateFormat.Format = dateFormat;
                    }

                    if (numberFormat != null)
                    {
                        cell.Style.NumberFormat.Format = numberFormat;
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
