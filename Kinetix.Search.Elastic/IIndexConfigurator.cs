using Elastic.Clients.Elasticsearch.IndexManagement;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Contrat des configurateurs d'index.
/// </summary>
public interface IIndexConfigurator
{
    /// <summary>
    /// Configure l'index.
    /// </summary>
    /// <param name="descriptor">Descripteur.</param>
    /// <returns>ICreateIndexRequest.</returns>
    void ConfigureIndex(CreateIndexRequestDescriptor descriptor);
}
