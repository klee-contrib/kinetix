using Kinetix.Services;

namespace Kinetix.Search.Model
{
    /// <summary>
    /// Facette de référence.
    /// </summary>
    /// <typeparam name="T">Type de la référence.</typeparam>
    public class ReferenceFacet<T> : IFacetDefinition
        where T : new()
    {
        private readonly IReferenceManager _referenceManager;

        public ReferenceFacet(IReferenceManager referenceManager)
        {
            _referenceManager = referenceManager;
        }

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

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public virtual string ResolveLabel(object primaryKey)
        {
            return _referenceManager.GetReferenceValue<T>(primaryKey);
        }
    }
}
