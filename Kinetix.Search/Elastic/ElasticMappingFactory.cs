using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Elastic.Mapping;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Usine à mapping ElasticSearch.
    /// </summary>
    public sealed class ElasticMappingFactory
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider"></param>
        public ElasticMappingFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Obtient le mapping de champ Elastic pour une catégorie donnée.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="T">Type du document.</typeparam>
        public PropertiesDescriptor<T> AddField<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptor field)
            where T : class
        {
            if (!(_provider.GetService(typeof(IElasticMapping<>).MakeGenericType(field.PropertyType)) is IElasticMapping mapper))
            {
                mapper = _provider.GetService<IElasticMapping<string>>();
            }

            switch (field.Category)
            {
                case SearchFieldCategory.Id:
                    return selector;
                case SearchFieldCategory.Facet:
                    return mapper.MapFacet(selector, field);
                case SearchFieldCategory.Filter:
                    return mapper.MapFilter(selector, field);
                case SearchFieldCategory.ListFacet:
                    return mapper.MapListFacet(selector, field);
                case SearchFieldCategory.Result:
                    return mapper.MapResult(selector, field);
                case SearchFieldCategory.Search:
                    return mapper.MapSearch(selector, field);
                case SearchFieldCategory.Security:
                    return mapper.MapSecurity(selector, field);
                case SearchFieldCategory.Sort:
                    return mapper.MapSort(selector, field);
                default:
                    throw new NotSupportedException($"Category not supported {field.Category}");
            }
        }
    }
}
