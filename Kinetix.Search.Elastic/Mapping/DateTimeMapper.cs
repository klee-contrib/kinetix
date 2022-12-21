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
    public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
        where TDocument : class
    {
        switch (field.Indexing)
        {
            case SearchFieldIndexing.Term:
            case SearchFieldIndexing.Sort:
                return selector.Date(x => x.Name(field.FieldName).Format("date_time_no_millis"));
            case SearchFieldIndexing.None:
                return selector.Date(x => x.Name(field.FieldName).Format("date_time_no_millis").Index(false));
            default:
                throw new NotSupportedException();
        }
    }
}
