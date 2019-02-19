using System;
using System.Collections.Generic;
using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Builder de requête ElasticSearch.
    /// </summary>
    public static class ElasticQueryBuilder
    {
        /// <summary>
        /// Construit une requête pour une recherche textuelle.
        /// </summary>
        /// <param name="text">Texte de recherche.</param>
        /// <param name="fields">Champs de recherche.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildMultiMatchQuery<TDocument>(string text, params string[] fields)
            where TDocument : class
        {
            return q => q.MultiMatch(m => m
                .Query(text)
                .Operator(Operator.And)
                .Type(TextQueryType.CrossFields)
                .Fields(fields));
        }

        /// <summary>
        /// Construit une requête pour inclure une valeur parmi plusieurs.
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="codes">Liste de valeurs à inclure.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildInclusiveInclude<TDocument>(string field, string codes)
            where TDocument : class
        {
            var clauses = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
            foreach (var word in codes.Split(' '))
            {
                clauses.Add(f => f.Term(t => t.Field(field).Value(word)));
            }

            return q => q.Bool(b => b.Should(clauses).MinimumShouldMatch(1));
        }

        /// <summary>
        /// Construit une requête pour exclure des valeurs.
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="codes">Liste de valeurs à exclure.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildExcludeQuery<TDocument>(string field, string codes)
            where TDocument : class
        {
            return q => q.Bool(b =>
            {
                var clauses = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
                foreach (var word in codes.Split(' '))
                {
                    clauses.Add(f => f.Term(t => t.Field(field).Value(word)));
                }

                return b.MustNot(clauses);
            });
        }

        /// <summary>
        /// Construit une requête pour le filtrage exacte (sélection de facette, ...).
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="value">Valeur.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildFilter<TDocument>(string field, string value)
            where TDocument : class
        {
            return q => q.Term(t => t.Field(field).Value(value));
        }

        /// <summary>
        /// Construit une requête pour un champ qui doit manquer (équivalent d'une valeur NULL).
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildMissingField<TDocument>(string field)
            where TDocument : class
        {
            return q => q.Bool(b => b.MustNot(m => m.Exists(t => t.Field(field))));
        }

        /// <summary>
        /// Construit une requête avec des AND sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildAndQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
            where TDocument : class
        {
            return q => q.Bool(b => b.Filter(subQueries));
        }

        /// <summary>
        /// Construit une requête avec des OR sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildOrQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
            where TDocument : class
        {
            return q => q.Bool(b => b
                .Should(subQueries)
                .MinimumShouldMatch(1));
        }
    }
}
