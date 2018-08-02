using System;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Decimal.
    /// </summary>
    public class DecimalMapping : IElasticMapping<decimal>
    {
        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapFullText<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapResult<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Number(x => x
                .Name(field.FieldName)
                .Index(false)
                .Store(true));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return MapTerm(selector, field);
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapTerm<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Number(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false));
        }

        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapTerms<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotSupportedException();
        }
    }
}
