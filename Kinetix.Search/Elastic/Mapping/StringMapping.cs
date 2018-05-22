using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs String.
    /// </summary>
    public class StringMapping : IElasticMapping<string>
    {
        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Keyword(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapFilter<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return MapFacet(selector, field);
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapListFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return MapSecurity(selector, field);
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapResult<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Text(x => x
                .Name(field.FieldName)
                .Index(false)
                .Store(true));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSearch<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Text(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false)
                .Analyzer("text_fr"));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSecurity<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return selector.Text(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(true)
                .Analyzer("text_fr"));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field) where TDocument : class
        {
            return MapFacet(selector, field);
        }
    }
}
