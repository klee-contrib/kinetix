using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs texte à indexer comme de l'anglais.
/// </summary>
public class EnglishMapper : IElasticMapper<string>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public Properties Map(Properties properties, DocumentFieldDescriptor field)
    {
        if (field.Indexing == SearchFieldIndexing.FullText)
        {
            properties.Add(field.FieldName, new TextProperty { Analyzer = "english" });
        }
        else
        {
            throw new NotSupportedException();
        }

        return properties;
    }
}
