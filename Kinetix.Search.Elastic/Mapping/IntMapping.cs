using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Mapping pour les champs Int.
    /// </summary>
    public class IntMapping : IElasticMapping<int>
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
                        .SearchAnalyzer("standard"));
                case SearchFieldIndexing.Term:
                case SearchFieldIndexing.Sort:
                    return selector.Number(x => x
                        .Name(field.FieldName)
                        .Type(NumberType.Integer));
                case SearchFieldIndexing.None:
                    return selector.Number(x => x
                        .Name(field.FieldName)
                        .Type(NumberType.Integer)
                        .Index(false));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
