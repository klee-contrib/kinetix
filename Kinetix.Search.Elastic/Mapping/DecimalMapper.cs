using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models.Annotations;
using Nest;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs Decimal.
/// </summary>
public class DecimalMapper : IElasticMapper<decimal>
{
    /// <inheritdoc cref="IElasticMapper.Map" />
    public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
        where TDocument : class
    {
        switch (field.Indexing)
        {
            case SearchFieldIndexing.FullText:
                return selector.Text(x => x.Name(field.FieldName).Analyzer("text").SearchAnalyzer("search_text"));
            case SearchFieldIndexing.Term:
            case SearchFieldIndexing.Sort:
                return selector.Number(x => x.Name(field.FieldName));
            case SearchFieldIndexing.None:
                return selector.Number(x => x.Name(field.FieldName).Index(false));
            default:
                throw new NotSupportedException();
        }
    }
}
