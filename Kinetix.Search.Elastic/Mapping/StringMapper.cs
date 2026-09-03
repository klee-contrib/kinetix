using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs String.
/// </summary>
public class StringMapper : IElasticMapper<string>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public Properties Map(Properties properties, DocumentFieldDescriptor field)
    {
        properties.Add(
            field.FieldName,
            field.Indexing == SearchFieldIndexing.FullText
                ? new TextProperty { Analyzer = "text", SearchAnalyzer = "search_text" }
                : new KeywordProperty
                {
                    Normalizer = field.Indexing == SearchFieldIndexing.Sort ? "keyword" : null,
                    Index = field.Indexing != SearchFieldIndexing.None,
                }
        );
        return properties;
    }
}
