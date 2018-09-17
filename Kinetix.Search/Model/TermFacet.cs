namespace Kinetix.Search.Model
{
    /// <summary>
    /// Facette de terme.
    /// </summary>
    public class TermFacet : IFacetDefinition
    {
        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public string Label { get; set; }

        /// <inheritdoc />
        public string FieldName { get; set; }

        /// <inheritdoc />
        public bool IsMultiSelectable { get; set; } = false;

        /// <inheritdoc />
        public bool HasMissing { get; set; } = true;

        /// <inheritdoc />
        public FacetOrdering Ordering { get; set; } = FacetOrdering.CountDescending;

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public virtual string ResolveLabel(object primaryKey)
        {
            return (string)primaryKey;
        }
    }
}
