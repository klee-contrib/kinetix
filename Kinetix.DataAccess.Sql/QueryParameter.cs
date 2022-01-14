using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Paramètre de tri des résultats et de limit des résultats.
    /// Les objets remontés sont triés par les valeurs associées à la map.
    /// </summary>
    public class QueryParameter
    {
        /// <summary>
        /// Liste des ordres de tri.
        /// </summary>
        private readonly List<string> _sortList = new();

        /// <summary>
        /// If manualSort.
        /// </summary>
        private bool? _isManualSort = false;

        /// <summary>
        /// Constructeur vide.
        /// </summary>
        public QueryParameter()
        {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        public QueryParameter(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        /// <param name="offset">Nombre de lignes à sauter.</param>
        public QueryParameter(int limit, int offset)
        {
            Limit = limit;
            Offset = offset;
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        public QueryParameter(Enum enumCol, SortOrder order)
        {
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        public QueryParameter(Enum enumCol, SortOrder order, int limit)
        {
            Limit = limit;
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        /// <param name="offset">Nombre de lignes à sauter.</param>
        public QueryParameter(Enum enumCol, SortOrder order, int limit, int offset)
        {
            Limit = limit;
            Offset = offset;
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Nombre de lignes à remener.
        /// </summary>
        public int Limit { get; set; } = 0;

        /// <summary>
        /// Nombre de lignes maximum.
        /// </summary>
        public int MaxRows { get; set; } = 0;

        /// <summary>
        /// Nombre de lignes à sauter.
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Retourne la liste des nom des paramètres.
        /// </summary>
        public ICollection<string> SortedFields => new ReadOnlyCollection<string>(_sortList);

        /// <summary>
        /// Indique si un tri manuel doit être effectué.
        /// </summary>
        public bool IsManualSort
        {
            get
            {
                if (_isManualSort == null)
                {
                    throw new NotSupportedException("Call ExcludeColumns first !");
                }

                return _isManualSort.Value;
            }
        }

        /// <summary>
        /// Define the sort traduction.
        /// </summary>
        /// <returns>Sort condition.</returns>
        public string SortCondition
        {
            get
            {
                var orderClause = new StringBuilder();
                var isFirst = true;
                foreach (var sort in _sortList)
                {
                    orderClause.Append(' ');
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        orderClause.Append(", ");
                    }

                    var orderCol = FirstToUpper(sort);
                    orderClause.Append('"' + orderCol + "\" " + ((MapSort[sort] == SortOrder.Desc) ? "desc" : "asc"));
                }

                return orderClause.ToString();
            }
        }

        /// <summary>
        /// Map des ordres de tri par paramètre.
        /// </summary>
        public Dictionary<string, SortOrder> MapSort { get; } = new();

        /// <summary>
        /// Retourne le tri associé au champ.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Ordre de tri.</returns>
        public SortOrder this[string fieldName] => MapSort[fieldName];

        /// <summary>
        /// Ajoute la chaine de tri.
        /// </summary>
        /// <param name="param">Param.</param>
        public void AddSortParam(string param)
        {
            if (param == null)
            {
                throw new ArgumentNullException("param");
            }

            var orderClause = param.Split(',');
            foreach (var orderby in orderClause)
            {
                var order = orderby.Split(' ');
                if (order.Length == 1 || order[1] != "desc")
                {
                    _sortList.Add(order[0]);
                    MapSort.Add(order[0], SortOrder.Asc);
                }
                else
                {
                    _sortList.Add(order[0]);
                    MapSort.Add(order[0], SortOrder.Desc);
                }
            }
        }

        /// <summary>
        /// Ajoute un critère de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri.</param>
        public void AddSortParam(Enum enumCol, SortOrder order)
        {
            if (enumCol == null)
            {
                throw new ArgumentNullException("enumCol");
            }

            var column = enumCol.ToString();
            _sortList.Add(column);
            MapSort.Add(column, order);
        }

        /// <summary>
        /// Applique le tri et le filtrage à la liste.
        /// </summary>
        /// <typeparam name="TSource">Type source.</typeparam>
        /// <param name="list">Liste source.</param>
        /// <returns>Liste triée.</returns>
        public ICollection<TSource> Apply<TSource>(ICollection<TSource> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (_sortList.Count != 1)
            {
                throw new NotImplementedException();
            }

            var sortColumn = _sortList[0];
            var sortOrder = MapSort[sortColumn];

            var beanDef = BeanDescriptor.GetDefinition(typeof(TSource));
            var propertyDescriptor = beanDef.Properties[FirstToUpper(sortColumn)];

            list = sortOrder == SortOrder.Asc
                ? list.OrderBy(x => propertyDescriptor.GetValue(x)).ToList()
                : list.OrderByDescending(x => propertyDescriptor.GetValue(x)).ToList();

            // If this.Limit == 0 we disable pagination.
            return list.Skip(Offset).Take(Limit > 0 ? Limit : list.Count).ToList();
        }

        /// <summary>
        /// Disable pagination.
        /// </summary>
        public void DisablePagination()
        {
            // TODO : Optimisation possible avec un paramètre client-side.
            Limit = 0;
            Offset = 0;
        }

        /// <summary>
        /// Retourne les paramètres à appliquer en cas de colonnes à exclure.
        /// </summary>
        /// <param name="columns">Liste des colonnes à exclure.</param>
        /// <returns>Paramètres.</returns>
        public QueryParameter ExcludeColumns(params string[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }

            foreach (var col in columns)
            {
                if (IsSortBy(col))
                {
                    _isManualSort = true;
                    return null;
                }
            }

            _isManualSort = false;
            return this;
        }

        /// <summary>
        /// Indique l'ordre de tri sur une colonne.
        /// </summary>
        /// <param name="memberName">Membre.</param>
        /// <returns>True si le tri se fait sur la colonne.</returns>
        public SortOrder GetSortOrder(string memberName)
        {
            return MapSort[memberName];
        }

        /// <summary>
        /// Indique si le tri est réalisé sur une colonne.
        /// </summary>
        /// <param name="memberName">Membre.</param>
        /// <returns>True si le tri se fait sur la colonne.</returns>
        public bool IsSortBy(string memberName)
        {
            return MapSort.ContainsKey(memberName);
        }

        /// <summary>
        /// Remap columns to persistant names.
        /// </summary>
        /// <param name="beanType">Bean.</param>
        public void RemapSortColumn(Type beanType)
        {
            if (beanType == null)
            {
                throw new ArgumentNullException("beanType");
            }

            var beanDef = BeanDescriptor.GetDefinition(beanType);
            foreach (var sort in _sortList.ToArray())
            {
                var orderCol = FirstToUpper(sort);
                if (beanDef.Properties.Contains(orderCol))
                {
                    var colName = beanDef.Properties[orderCol].MemberName;
                    if (!string.IsNullOrEmpty(colName))
                    {
                        RemapSortColumn(sort, colName);
                    }
                }
            }
        }

        /// <summary>
        /// Remap column name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="columnName">Column name.</param>
        public void RemapSortColumn(string propertyName, string columnName)
        {
            // Lecture de l'état initial.
            var index = _sortList.IndexOf(propertyName);
            var order = MapSort[propertyName];

            // Suppression de l'état initial.
            _sortList.RemoveAt(index);
            MapSort.Remove(propertyName);

            // Création du nouvel état.
            _sortList.Insert(index, columnName);
            MapSort[columnName] = order;
        }

        /// <summary>
        /// Returns the value wih the first character uppered.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <returns>Parsed value.</returns>
        private static string FirstToUpper(string value)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : value[..1].ToUpper(CultureInfo.CurrentCulture) + value[1..];
        }
    }
}
