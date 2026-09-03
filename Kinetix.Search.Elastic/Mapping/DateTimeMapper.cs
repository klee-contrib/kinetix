using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs Date.
/// </summary>
public class DateTimeMapper : IElasticMapper<DateTime>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public Properties Map(Properties properties, DocumentFieldDescriptor field)
    {
        properties.Add(
            field.FieldName,
            new DateProperty { Format = "date_time_no_millis", Index = field.Indexing != SearchFieldIndexing.None }
        );
        return properties;
    }
}
