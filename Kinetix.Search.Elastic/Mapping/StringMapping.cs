using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs String.
    /// </summary>
    public class StringMapping : IElasticMapping<string>
    {
        /// <inheritdoc cref="IElasticMapping.Map" />
        public PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class
        {
            switch (field.Indexing)
            {
                case SearchFieldIndexing.FullText:
                    return selector.Text(x => x
                        .Name(field.FieldName)
                        .Analyzer("text")
                        .SearchAnalyzer("search_text"));
                case SearchFieldIndexing.Term:
                case SearchFieldIndexing.Sort:
                    return selector.Keyword(x => x.Name(field.FieldName));
                case SearchFieldIndexing.None:
                    return selector.Text(x => x
                        .Name(field.FieldName)
                        .Index(false));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
