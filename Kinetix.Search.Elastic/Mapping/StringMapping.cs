using Kinetix.Search.DocumentModel;
using Kinetix.Search.Models.Annotations;
using Nest;

namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Mapping pour les champs String.
/// </summary>
public class StringMapping : IElasticMapping<string>
{
    /// <inheritdoc cref="IElasticMapping.Map" />
    public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
        where TDocument : class
    {
        return field.Indexing switch
        {
            SearchFieldIndexing.FullText =>
                selector.Text(x => x.Name(field.FieldName).Analyzer("text").SearchAnalyzer("search_text")),
            SearchFieldIndexing.Term =>
                selector.Keyword(x => x.Name(field.FieldName)),
            SearchFieldIndexing.Sort =>
                selector.Keyword(x => x.Name(field.FieldName).Normalizer("keyword")),
            SearchFieldIndexing.None =>
                selector.Text(x => x.Name(field.FieldName).Index(false)),
            _ => throw new NotSupportedException(),
        };
    }
}
