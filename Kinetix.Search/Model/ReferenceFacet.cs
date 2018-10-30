using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ComponentModel;
using Kinetix.Search.ComponentModel;
using Kinetix.Services;

namespace Kinetix.Search.Model
{
    public abstract class ReferenceFacet : IFacetDefinition
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

        /// <summary>
        /// Affiche l'intégralité des valeurs de la liste de référence dans les résultats de facettes, même si les buckets sont vides.
        /// Nécessite le BeanDescriptor.
        /// </summary>
        public bool ShowEmptyReferenceValues { get; set; } = false;

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public abstract string ResolveLabel(object primaryKey);

        /// <summary>
        /// Récupère la liste de référence associée à la facette.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<FacetItem> GetReferenceList();
    }

    /// <summary>
    /// Facette de référence.
    /// </summary>
    /// <typeparam name="T">Type de la référence.</typeparam>
    public class ReferenceFacet<T> : ReferenceFacet
        where T : new()
    {
        private readonly IReferenceManager _referenceManager;
        private readonly BeanDescriptor _beanDescriptor;

        /// <summary>
        /// Construit une facette de liste de référénce.
        /// </summary>
        /// <param name="referenceManager">ReferenceManager.</param>
        /// <param name="beanDescriptor">BeanDescriptor, nécessaire si ShowEmptyReferenceValues = true.</param>
        public ReferenceFacet(IReferenceManager referenceManager, BeanDescriptor beanDescriptor = null)
        {
            _referenceManager = referenceManager;
            _beanDescriptor = beanDescriptor;
        }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public override string ResolveLabel(object primaryKey)
        {
            return _referenceManager.GetReferenceValue<T>(primaryKey);
        }

        /// <inheritdoc />
        public override IEnumerable<FacetItem> GetReferenceList()
        {
            if (_beanDescriptor == null)
            {
                throw new InvalidOperationException($"Veuillez renseigner le BeanDescriptor avec ShowEmptyReferenceValues = true pour la facette sur le type {typeof(T).Name}");
            }

            var def = _beanDescriptor.GetDefinition(typeof(T));
            return _referenceManager.GetReferenceList<T>()
                .Select(item => new FacetItem
                {
                    Code = def.PrimaryKey.GetValue(item).ToString(),
                    Label = (string)def.DefaultProperty.GetValue(item),
                    Count = 0
                });
        }
    }
}
