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
        /// Effectue le mapping pour les champs d'un document.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="fields">Les champs.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="T">Type du document.</typeparam>
        public PropertiesDescriptor<T> AddFields<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptorCollection fields)
             where T : class
        {
            foreach (var field in fields)
            {
                AddField(selector, field);
            }

            return selector;
        }

        /// <summary>
        /// Effectue le mapping pour un champ d'un document.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Le champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="T">Type du document.</typeparam>
        public PropertiesDescriptor<T> AddField<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptor field)
            where T : class
        {
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
