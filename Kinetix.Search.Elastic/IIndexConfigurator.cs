using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Contrat des configurateurs d'index.
    /// </summary>
    public interface IIndexConfigurator
    {
        /// <summary>
        /// Paramètres de l'index.
        /// </summary>
        IIndexSettings IndexSettings { get; }

        /// <summary>
        /// Crée la requête de création d'index.
        /// </summary>
        /// <param name="indexName">Nom de l'index.</param>
        /// <returns>Requête de création d'index.</returns>
        ICreateIndexRequest CreateIndexRequest(IndexName indexName);
    }
}
