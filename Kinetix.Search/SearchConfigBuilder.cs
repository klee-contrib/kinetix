using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Kinetix.Search
{
    public class SearchConfigBuilder
    {
        private readonly IServiceCollection _services;

        internal SearchConfigBuilder(IServiceCollection services)
        {
            _services = services;
            AddMapping<DateTimeMapping>();
            AddMapping<DecimalMapping>();
            AddMapping<IntMapping>();
            AddMapping<StringMapping>();
        }

        internal string DefaultDataSourceName { get; set; }

        internal ICollection<Type> DocumentTypes { get; } = new List<Type>();

        internal ICollection<JsonConverter> JsonConverters { get; } = new List<JsonConverter>();

        public SearchConfigBuilder AddDocumentType<T>()
        {
            DocumentTypes.Add(typeof(T));
            return this;
        }

        public SearchConfigBuilder AddElasticMapping<T>()
            where T : class, IElasticMapping
        {
            return AddMapping<T>();
        }

        public SearchConfigBuilder AddJsonConverter<T>()
            where T : JsonConverter, new()
        {
            JsonConverters.Add(new T());
            return this;
        }

        public SearchConfigBuilder UseDefaultDataSource(string name)
        {
            DefaultDataSourceName = name;
            return this;
        }

        private SearchConfigBuilder AddMapping<T>()
            where T : class, IElasticMapping
        {
            _services.AddSingleton(typeof(T).GetInterfaces().First(), typeof(T));
            return this;
        }
    }
}
