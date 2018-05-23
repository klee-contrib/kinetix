using System;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Date.
    /// </summary>
    public class DateTimeMapping : IElasticMapping<DateTime?>
    {
        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapFilter<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapListFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotImplementedException();
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
        public virtual PropertiesDescriptor<TDocument> MapSearch<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSecurity<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Date(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false));
        }
    }
}
