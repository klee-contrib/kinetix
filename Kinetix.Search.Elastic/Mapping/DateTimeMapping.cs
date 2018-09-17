using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Date.
    /// </summary>
    public class DateTimeMapping : IElasticMapping<DateTime>
    {
        /// <inheritdoc cref="IElasticMapping.Map" />
        public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            switch (field.Indexing)
            {
                case SearchFieldIndexing.Sort:
                    return selector.Date(x => x
                        .Name(field.FieldName));
                case SearchFieldIndexing.None:
                    return selector.Date(x => x
                        .Name(field.FieldName)
                        .Index(false));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
