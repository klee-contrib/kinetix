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
    public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
        where TDocument : class
    {
        if (field.Indexing == SearchFieldIndexing.FullText)
        {
            return selector.Text(x => x.Name(field.FieldName).Analyzer("english"));
        }
        else
        {
            throw new NotSupportedException();
        };
    }
}
