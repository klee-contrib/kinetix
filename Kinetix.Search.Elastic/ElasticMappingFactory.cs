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
            // On skip le mapping pour les types génériques, par exemple les arrays.
            if (field.PropertyType.IsGenericType && field.PropertyType.Name != "Nullable`1")
            {
                return selector;
            }

            if (!(_provider.GetService(typeof(IElasticMapping<>).MakeGenericType(field.PropertyType)) is IElasticMapping mapper))
            {
                mapper = _provider.GetService<IElasticMapping<string>>();
            }

            switch (field.SearchCategory)
            {
                case SearchFieldCategory.FullText:
                    return mapper.MapFullText(selector, field);
                case SearchFieldCategory.Result:
                    return mapper.MapResult(selector, field);
                case SearchFieldCategory.Sort:
                    return mapper.MapSort(selector, field);
                case SearchFieldCategory.Term:
                    return mapper.MapTerm(selector, field);
                case SearchFieldCategory.Terms:
                    return mapper.MapTerms(selector, field);
                default:
                    return selector;
            }
        }
    }
}
