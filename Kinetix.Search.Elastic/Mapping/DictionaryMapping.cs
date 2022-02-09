using System;
using System.Collections.Generic;
using Kinetix.Search.Attributes;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Dictionary.
    /// </summary>
    public class DictionaryMapping : IElasticMapping<Dictionary<string, string>>
    {
        /// <inheritdoc cref="IElasticMapping.Map" />
        public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return field.Indexing switch
            {
                SearchFieldIndexing.None => selector.Object<object>(x => x.Name(field.FieldName)),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
