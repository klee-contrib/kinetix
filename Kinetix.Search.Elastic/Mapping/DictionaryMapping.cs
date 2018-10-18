using System;
using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
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
            switch (field.Indexing)
            {
                case SearchFieldIndexing.None:
                    return selector.Object<object>(x => x.Name(field.FieldName));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
