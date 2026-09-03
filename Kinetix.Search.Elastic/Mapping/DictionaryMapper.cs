using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs Dictionary.
/// </summary>
public class DictionaryMapper : IElasticMapper<Dictionary<string, string>>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public Properties Map(Properties properties, DocumentFieldDescriptor field)
    {
        properties.Add(field.FieldName, new ObjectProperty());
        return properties;
    }
}
