using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs Int.
/// </summary>
public class IntMapper : IElasticMapper<int>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public Properties Map(Properties properties, DocumentFieldDescriptor field)
    {
        properties.Add(
            field.FieldName,
            field.Indexing == SearchFieldIndexing.FullText
                ? new TextProperty { Analyzer = "text", SearchAnalyzer = "search_text" }
                : new IntegerNumberProperty { Index = field.Indexing != SearchFieldIndexing.None }
        );
        return properties;
    }
}
