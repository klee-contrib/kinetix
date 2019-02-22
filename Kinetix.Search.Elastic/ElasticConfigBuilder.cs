using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Configurateur pour ElasticSearch.
    /// </summary>
    public class ElasticConfigBuilder
    {
        internal const string ServerName = "Elastic6";

        private readonly IServiceCollection _services;

        internal ElasticConfigBuilder(IServiceCollection services)
        {
            _services = services;
            AddMapping<DateTimeMapping>();
            AddMapping<DecimalMapping>();
            AddMapping<IntMapping>();
            AddMapping<StringMapping>();
            AddMapping<DictionaryMapping>();
        }

        internal ICollection<Type> DocumentTypes { get; } = new List<Type>();

        internal ICollection<JsonConverter> JsonConverters { get; } = new List<JsonConverter>();

        /// <summary>
        /// Enregistre un index en lecture pour un document.
        /// </summary>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        /// <returns>Builder.</returns>
        public ElasticConfigBuilder AddDocumentType<TDocument>()
        {
            DocumentTypes.Add(typeof(TDocument));
            return this;
        }

        /// <summary>
        /// Enregistre un index en lecture et en écriture pour un document.
        /// </summary>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        /// <typeparam name="TLoader">DocumentLoader pour le document.</typeparam>
        /// <returns>Builder.</returns>
        public ElasticConfigBuilder AddDocumentType<TDocument, TLoader>()
            where TLoader : class, IDocumentLoader<TDocument>
        {
            DocumentTypes.Add(typeof(TDocument));
            _services.AddScoped<IDocumentLoader<TDocument>, TLoader>();
            return this;
        }

        /// <summary>
        /// Ajoute un mapping pour un type de champ.
        /// </summary>
        /// <typeparam name="TMappping">Type de champ.</typeparam>
        /// <returns>Builder.</returns>
        public ElasticConfigBuilder AddMapping<TMappping>()
            where TMappping : class, IElasticMapping
        {
            _services.AddSingleton(typeof(TMappping).GetInterfaces().First(), typeof(TMappping));
            return this;
        }

        /// <summary>
        /// Ajoute un converter Json pour un type de champ.
        /// </summary>
        /// <typeparam name="TJsonConverter">JsonConverter</typeparam>
        /// <returns>Builder.</returns>
        public ElasticConfigBuilder AddJsonConverter<TJsonConverter>()
            where TJsonConverter : JsonConverter, new()
        {
            JsonConverters.Add(new TJsonConverter());
            return this;
        }
    }
}
