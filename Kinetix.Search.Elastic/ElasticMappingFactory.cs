using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Usine à mapping ElasticSearch.
/// </summary>
public sealed class ElasticMappingFactory(IServiceProvider provider)
{
    /// <summary>
    /// Effectue le mapping pour un champ d'un document.
    /// </summary>
    /// <param name="properties">Descripteur des propriétés.</param>
    /// <param name="field">Le champ.</param>
    /// <returns>Mapping de champ.</returns>
    public Properties AddField(Properties properties, DocumentFieldDescriptor field)
    {
        var mapperType = field.OtherAttributes.OfType<ElasticMapperAttribute>().FirstOrDefault()?.MapperType;

        if (mapperType != null)
        {
            return ((IElasticMapper)Activator.CreateInstance(mapperType)!).Map(properties, field);
        }

        if (
            provider.GetService(typeof(IElasticMapper<>).MakeGenericType(field.PropertyType))
            is not IElasticMapper mapper
        )
        {
            mapper = provider.GetRequiredService<IElasticMapper<string>>();
        }

        return mapper.Map(properties, field);
    }

    /// <summary>
    /// Effectue le mapping pour les champs d'un document.
    /// </summary>
    /// <param name="properties">Descripteur des propriétés.</param>
    /// <param name="fields">Les champs.</param>
    /// <returns>Mapping de champ.</returns>
    public Properties AddFields(Properties properties, DocumentFieldDescriptorCollection fields)
    {
        foreach (var field in fields.OrderBy(field => field.FieldName))
        {
            AddField(properties, field);
        }

        return properties;
    }
}
