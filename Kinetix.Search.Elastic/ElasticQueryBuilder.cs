using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Builder de requête ElasticSearch.
    /// </summary>
    public class ElasticQueryBuilder
    {
        /// <summary>
        /// Caractères réservés de la syntaxe Query DSL à échapper.
        /// </summary>
        private static readonly string[] elasticSpecialChars = new string[] { "<", ">", "=", "/", "\\", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "~", "*", "?", ":", "." };

        /// <summary>
        /// Construit une requête pour une recherche textuelle.
        /// </summary>
        /// <param name="text">Texte de recherche.</param>
        /// <param name="fields">Champs de recherche.</param>
        /// <returns>Requête.</returns>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildFullTextSearch<TDocument>(string text, params string[] fields)
            where TDocument : class
        {
            if (string.IsNullOrEmpty(text))
            {
                return q => q;
            }

            /* Enlève les accents. */
            var withoutAccent = RemoveDiacritics(text);
            /* Passe en minsucule. */
            var lower = withoutAccent.ToLower(CultureInfo.CurrentCulture);
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(lower);
            /* Remplace les tirets et apostrophe par des espaces. */
            escapedValue = escapedValue.Replace('-', ' ').Replace('\'', ' ');

            return q => q.MultiMatch(m => m
                .Query(text)
                .Type(TextQueryType.PhrasePrefix)
                .Fields(fields)
                .MaxExpansions(1000)
                .Slop(2));
        }

        /// <summary>
        /// Construit une requête pour inclure une valeur parmi plusieurs.
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="codes">Liste de valeurs à inclure.</param>
        /// <returns>Requête.</returns>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildInclusiveInclude<TDocument>(string field, string codes)
            where TDocument : class
        {
            var escapedValue = EscapeLuceneSpecialChars(codes);
            var clauses = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
            foreach (var word in escapedValue.Split(' '))
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
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildExcludeQuery<TDocument>(string field, string codes)
            where TDocument : class
        {
            var escapedValue = EscapeLuceneSpecialChars(codes);
            return q => q.Bool(b =>
            {
                var clauses = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
                foreach (var word in escapedValue.Split(' '))
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
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildFilter<TDocument>(string field, string value)
            where TDocument : class
        {
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(value);
            return q => q.Term(t => t.Field(field).Value(escapedValue));
        }

        /// <summary>
        /// Construit une requête pour un champ qui doit manquer (équivalent d'une valeur NULL).
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <returns>Requête.</returns>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildMissingField<TDocument>(string field)
            where TDocument : class
        {
            return q => q.Bool(b => b.MustNot(m => m.Exists(t => t.Field(field))));
        }

        /// <summary>
        /// Construit une requête avec des AND sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildAndQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
            where TDocument : class
        {
            return q => q.Bool(b => b.Filter(subQueries));
        }

        /// <summary>
        /// Construit une requête avec des OR sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildOrQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
            where TDocument : class
        {
            return q => q.Bool(b => b
                .Should(subQueries)
                .MinimumShouldMatch(1));
        }

        /// <summary>
        /// Échappe les caractères spéciaux ElasticSearch.
        /// </summary>
        /// <param name="value">Texte à traiter.</param>
        /// <returns>Chaîne échappée.</returns>
        private static string EscapeLuceneSpecialChars(string value)
        {
            var sb = new StringBuilder(value);
            foreach (var specialChar in elasticSpecialChars)
            {
                sb.Replace(specialChar, @"\" + specialChar);
            }

            sb.Replace("\"", string.Empty);

            return sb.ToString();
        }

        /// <summary>
        /// Remplace les caractères avec accents par les caractères correspondants sans accents.
        /// </summary>
        /// <param name="raw">Chaîne brute.</param>
        /// <returns>Chaîne traitée.</returns>
        private static string RemoveDiacritics(string raw)
        {
            var normalizedString = raw.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
