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
        public PropertiesDescriptor<TDocument> MapFullText<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return selector.Text(x => x
               .Name(field.FieldName)
               .Index(true)
               .Store(false)
               .Analyzer("text_fr"));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapResult<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return selector.Text(x => x
                .Name(field.FieldName)
                .Index(false)
                .Store(true));
        }

        /// <inheritdoc />
        public virtual PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return MapTerm(selector, field);
        }

        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapTerm<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return selector.Keyword(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(false));
        }

        /// <inheritdoc />
        public PropertiesDescriptor<TDocument> MapTerms<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            return selector.Text(x => x
                .Name(field.FieldName)
                .Index(true)
                .Store(true)
                .Analyzer("text_fr")
                .Fielddata(true));
        }
    }
}
