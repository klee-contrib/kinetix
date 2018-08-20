using System;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Date.
    /// </summary>
    public class DateTimeMapping : IElasticMapping<DateTime>
    {
        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapFullText<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapResult<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Date(x => x
                .Name(field.FieldName)
                .Index(false)
                .Store(true));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Date(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false));
        }

        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapTerm<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapTerms<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            throw new NotSupportedException();
        }
    }
}
