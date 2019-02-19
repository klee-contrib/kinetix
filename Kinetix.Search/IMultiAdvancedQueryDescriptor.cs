using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search
{
    /// <summary>
    /// Descripteur pour une requête de recherche avancée multiple.
    /// </summary>
    public interface IMultiAdvancedQueryDescriptor
    {
        /// <summary>
        /// Ajoute une recherche avancée dans la requête.
        /// </summary>
        /// <param name="code">Code du groupe.</param>
        /// <param name="label">Libellé du groupe.</param>
        /// <param name="input">Input de la recherche.</param>
        /// <param name="documentMapper">Mapper de document.</param>
        /// <returns>Descriptor.</returns>
        IMultiAdvancedQueryDescriptor AddQuery<TDocument, TOutput, TCriteria>(string code, string label, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
            where TCriteria : Criteria, new();

        /// <summary>
        /// Effectue la requête.
        /// </summary>
        /// <returns>QueryOutput.</returns>
        QueryOutput Search();
    }
}