using System;
using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Faceting
{
    using static ElasticQueryBuilder;

    /// <summary>
    /// Handler de facette Portfolio.
    /// </summary>
    public class PortfolioFacetHandler : IFacetHandler
    {
        private const string InValue = "1";
        private const string OutValue = "0";
        private readonly DocumentDescriptor _documentDescriptor;

        /// <summary>
        /// Créé une nouvelle instance de PortfolioFacetHandler.
        /// </summary>
        /// <param name="document">Définition du document.</param>
        public PortfolioFacetHandler(DocumentDescriptor documentDescriptor)
        {
            _documentDescriptor = documentDescriptor;
        }

        /// <inheritdoc/>
        public void DefineAggregation<TDocument>(AggregationContainerDescriptor<TDocument> agg, IFacetDefinition facet, ICollection<IFacetDefinition> facetList, FacetListInput selectedFacets, string portfolio)
            where TDocument : class
        {
            if (string.IsNullOrEmpty(portfolio))
            {
                /* Portefeuille de l'utilisateur vide : on ne définit pas d'agregations. */
                return;
            }

            /* Récupère le nom du champ. */
            var fieldName = _documentDescriptor.GetDefinition(typeof(TDocument)).Fields[facet.FieldName].FieldName;

            /* On construit la requête de filtrage sur les autres facettes multi-sélectionnables. */
            var filterQuery = FacetingUtil.BuildMultiSelectableFacetFilter(facet, facetList, selectedFacets, CreateFacetSubQuery<TDocument>);

            /* Créé une agrégation avec deux buckets. */
            var inQuery = BuildAndQuery(BuildInclusiveInclude<TDocument>(fieldName, portfolio), filterQuery);
            var outQuery = BuildAndQuery(BuildExcludeQuery<TDocument>(fieldName, portfolio), filterQuery);

            agg.Filters(facet.Code, st => st.NamedFilters(x => x
                /* Une pour les documents dans le portefeuille */
                .Filter(InValue, inQuery)
                /* Une pour les documents absents du portefeuille */
                .Filter(OutValue, outQuery)));
        }

        /// <inheritdoc />
        public ICollection<FacetItem> ExtractFacetItemList(AggregateDictionary aggs, IFacetDefinition facetDef, long total)
        {
            var facetOutput = new List<FacetItem>();
            /* Valeurs renseignées. */
            var agg = aggs.Filters(facetDef.Code);

            /* Cas où le portefeuille de l'utilisateur est vide : l'agrégation n'est pas définie */
            if (agg == null)
            {

                /* On considère que tous les documents sont en dehors du portefeuille. */
                facetOutput.Add(new FacetItem { Code = OutValue, Label = facetDef.ResolveLabel(OutValue), Count = total });

                return facetOutput;
            }

            /* Pour les valeurs inclus / exclus */
            foreach (var facetName in new[] { InValue, OutValue })
            {

                /* Ajoute une output s'il y a des documents sur la facet. */
                var bucket = agg.NamedBucket(facetName);
                if (bucket.DocCount > 0)
                {
                    facetOutput.Add(new FacetItem { Code = facetName, Label = facetDef.ResolveLabel(facetName), Count = bucket.DocCount });
                }
            }

            return facetOutput;
        }

        /// <inheritdoc/>
        public void CheckFacet<TDocument>(IFacetDefinition facetDef)
            where TDocument : class
        {
            var doc = _documentDescriptor.GetDefinition(typeof(TDocument));
            if (!doc.Fields.HasProperty(facetDef.FieldName))
            {
                throw new ElasticException("The Document \"" + typeof(TDocument) + "\" is missing a \"" + facetDef.FieldName + "\" property to facet on.");
            }
        }

        /// <inheritdoc/>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> CreateFacetSubQuery<TDocument>(string facet, IFacetDefinition facetDef, string portfolio)
            where TDocument : class
        {
            var fieldDesc = _documentDescriptor.GetDefinition(typeof(TDocument)).Fields[facetDef.FieldName];
            var fieldName = fieldDesc.FieldName;

            if (string.IsNullOrEmpty(portfolio))
            {
                /* Utilisateur avec un portefeuille vide : on ne filtre pas sur la facette. */
                return q => q;
            }

            switch (facet)
            {
                case InValue:
                    return BuildInclusiveInclude<TDocument>(fieldName, portfolio);
                case OutValue:
                    return BuildExcludeQuery<TDocument>(fieldName, portfolio);
                default:
                    return q => q;
            }
        }
    }
}
