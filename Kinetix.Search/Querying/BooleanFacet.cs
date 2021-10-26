using System;
using System.Linq.Expressions;

namespace Kinetix.Search.Querying
{
    /// <summary>
    /// Facette de booléen.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    public class BooleanFacet<TDocument> : TermFacet<TDocument>
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="code">Code de la facette.</param>
        /// <param name="label">Libellé de la facette.</param>
        /// <param name="field">Champ sur lequel agit la facette.</param>
        public BooleanFacet(string code, string label, Expression<Func<TDocument, object>> field)
            : base(code, label, field)
        {
        }

        /// <inheritdoc cref="IFacetDefinition.IsMultiSelectable" />
        public override bool IsMultiSelectable => false;

        /// <inheritdoc cref="IFacetDefinition.CanExclude" />
        public override bool CanExclude => false;

        /// <inheritdoc cref="IFacetDefinition.Ordering" />
        public override FacetOrdering Ordering => FacetOrdering.KeyDescending;

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public override string ResolveLabel(string primaryKey)
        {
            return primaryKey == "1" || primaryKey == "true"
                ? "focus.search.results.yes"
                : "focus.search.results.no";
        }
    }
}
