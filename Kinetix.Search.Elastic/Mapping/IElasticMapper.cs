#pragma warning disable S2326

using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Définit un mapping pour un type de champ, selon sa catégorie.
/// </summary>
/// <typeparam name="T">Type du champ pour le mapping.</typeparam>
public interface IElasticMapper<T> : IElasticMapper { }

/// <summary>
/// Définit un mapping pour un type de champ, selon sa catégorie.
/// </summary>
public interface IElasticMapper
{
    /// <summary>
    /// Définit le mapping pour le type..
    /// </summary>
    /// <param name="properties">Descripteur des propriétés.</param>
    /// <param name="field">Catégorie de champ.</param>
    /// <returns>Mapping de champ.</returns>
    Properties Map(Properties properties, DocumentFieldDescriptor field);
}
