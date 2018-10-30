using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Faceting
{
    using static ElasticQueryBuilder;

    /// <summary>
    /// Handler de facette standard.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class StandardFacetHandler<TDocument> : IFacetHandler<TDocument>
        where TDocument : class
    {
        private const string MissingFacetPrefix = "_Missing";
        private readonly DocumentDefinition _document;

        /// <summary>
        /// Créé une nouvelle instance de StandardFacetHandler.
        /// </summary>
        /// <param name="document">Définition du document.</param>
        public StandardFacetHandler(DocumentDefinition document)
        {
            _document = document;
        }

        /// <inheritdoc/>
        public void DefineAggregation(AggregationContainerDescriptor<TDocument> agg, IFacetDefinition facet, ICollection<IFacetDefinition> facetList, FacetListInput selectedFacets, string portfolio)
        {
            /* Récupère le nom du champ. */
            var fieldName = _document.Fields[facet.FieldName].FieldName;

            /* On construit la requête de filtrage sur les autres facettes multi-sélectionnables. */
            var filterQuery = FacetingUtil.BuildMultiSelectableFacetFilter(facet, facetList, selectedFacets, CreateFacetSubQuery);
            var hasFilter = FacetingUtil.HasFilter(facet, facetList, selectedFacets);

            AggregationContainerDescriptor<TDocument> AggDescriptor(AggregationContainerDescriptor<TDocument> aa)
            {
                /* Crée une agrégation sur les valeurs discrètes du champ. */
                aa.Terms(facet.Code, st => st
                    .Field(fieldName)
                    .Size(50)
                    .Order(t =>
                    {
                        switch (facet.Ordering)
                        {
                            case FacetOrdering.KeyAscending:
                                return t.KeyAscending();
                            case FacetOrdering.KeyDescending:
                                return t.KeyDescending();
                            case FacetOrdering.CountAscending:
                                return t.CountAscending();
                            default:
                                return t.CountDescending();
                        }
                    }));

                /* Crée une agrégation pour les valeurs non renseignées du champ. */
                if (facet.HasMissing)
                {
                    aa.Missing(facet.Code + MissingFacetPrefix, ad => ad.Field(fieldName));
                }

                return aa;
            };

            if (!hasFilter)
            {
                AggDescriptor(agg);
            }
            else
            {
                agg.Filter(facet.Code, f => f
                    /* Crée le filtre sur les facettes multi-sélectionnables. */
                    .Filter(filterQuery)
                    .Aggregations(AggDescriptor));
            }
        }

        /// <inheritdoc />
        public ICollection<FacetItem> ExtractFacetItemList(AggregateDictionary aggs, IFacetDefinition facetDef, long total)
        {
            var facetOutput = new List<FacetItem>();

            /* Valeurs renseignées. */
            var bucket = aggs.Terms(facetDef.Code);
            if (bucket == null)
            {
                bucket = aggs.Filter(facetDef.Code).Terms(facetDef.Code);
            }

            foreach (var b in bucket.Buckets)
            {
                facetOutput.Add(new FacetItem { Code = b.Key, Label = facetDef.ResolveLabel(b.Key), Count = b.DocCount ?? 0 });
            }

            /* Valeurs non renseignées. */
            if (facetDef.HasMissing)
            {
                var missingBucket = aggs.Missing(facetDef.Code + MissingFacetPrefix);
                if (missingBucket == null)
                {
                    missingBucket = aggs.Filter(facetDef.Code).Missing(facetDef.Code + MissingFacetPrefix);
                }

                var missingCount = missingBucket.DocCount;
                if (missingCount > 0)
                {
                    facetOutput.Add(new FacetItem { Code = FacetConst.NullValue, Label = "focus.search.results.missing", Count = missingCount });
                }
            }

            // Gestion du mode "affichage de toutes les valeurs de listes de référence".
            if (facetDef is ReferenceFacet rfDef && rfDef.ShowEmptyReferenceValues)
            {
                facetOutput.AddRange(rfDef.GetReferenceList().Where(rlItem => !facetOutput.Any(fi => fi.Code == rlItem.Code)));

                // On est obligé de retrier par derrière.
                switch (facetDef.Ordering)
                {
                    case FacetOrdering.CountAscending:
                        return facetOutput.OrderBy(fi => fi.Count).ToList();
                    case FacetOrdering.CountDescending:
                        return facetOutput.OrderByDescending(fi => fi.Count).ToList();
                    case FacetOrdering.KeyAscending:
                        return facetOutput.OrderBy(fi => fi.Code).ToList();
                    case FacetOrdering.KeyDescending:
                        return facetOutput.OrderByDescending(fi => fi.Code).ToList();
                }
            }

            return facetOutput;
        }

        /// <inheritdoc/>
        public void CheckFacet(IFacetDefinition facetDef)
        {
            if (!_document.Fields.HasProperty(facetDef.FieldName))
            {
                throw new ElasticException("The Document \"" + _document.DocumentTypeName + "\" is missing a \"" + facetDef.FieldName + "\" property to facet on.");
            }
        }

        /// <inheritdoc/>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> CreateFacetSubQuery(string facet, IFacetDefinition facetDef, string portfolio)
        {
            var fieldDesc = _document.Fields[facetDef.FieldName];
            var fieldName = fieldDesc.FieldName;

            /* Traite la valeur de sélection NULL */
            return facet == FacetConst.NullValue
                ? BuildMissingField<TDocument>(fieldName)
                : BuildFilter<TDocument>(fieldName, facet);
        }
    }
}
