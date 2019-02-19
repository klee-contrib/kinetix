using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Elastic.Faceting;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Querying
{
    using static AdvancedQueryUtil;

    public class MultiAdvancedQueryDescriptor : IMultiAdvancedQueryDescriptor
    {
        private readonly ElasticClient _client;
        private readonly DocumentDescriptor _documentDescriptor;
        private readonly Func<IFacetDefinition, IFacetHandler> _getFacetHandler;
        private readonly IDictionary<string, IDocumentMapper> _documentMappers = new Dictionary<string, IDocumentMapper>();
        private readonly IDictionary<string, ISearchRequest> _searchDescriptors = new Dictionary<string, ISearchRequest>();
        private readonly IList<(string code, string label)> _searchLabels = new List<(string, string)>();

        public MultiAdvancedQueryDescriptor(ElasticClient client, DocumentDescriptor documentDescriptor, Func<IFacetDefinition, IFacetHandler> getFacetHandler)
        {
            _client = client;
            _documentDescriptor = documentDescriptor;
            _getFacetHandler = getFacetHandler;
        }

        /// <inheritdoc cref="IMultiAdvancedQueryDescriptor.AddQuery" />
        public IMultiAdvancedQueryDescriptor AddQuery<TDocument, TOutput, TCriteria>(string code, string label, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
            where TCriteria : Criteria, new()
        {
            input.ApiInput.Group = null;

            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            _searchDescriptors.Add(code, GetAdvancedQueryDescriptor(
                def,
                input,
                _getFacetHandler,
                GetFacetDefinitionList(input, _getFacetHandler),
                GetGroupFieldName(def, input),
                new Func<QueryContainerDescriptor<TDocument>, QueryContainer>[0])(new SearchDescriptor<TDocument>()));
            _documentMappers.Add(code, new DocumentMapper<TDocument, TOutput>(documentMapper));
            _searchLabels.Add((code, label));
            return this;
        }

        /// <inheritdoc cref="IMultiAdvancedQueryDescriptor.Search" />
        public QueryOutput Search()
        {
            var response = _client.MultiSearch(new MultiSearchRequest { Operations = _searchDescriptors });

            /* Extraction des résultats. */
            var groups = response.AllResponses.Select((dynamic res, int i) =>
                 new GroupResult
                 {
                     Code = _searchLabels[i].code,
                     Label = _searchLabels[i].label,
                     List = ((ICollection)res.Documents).Cast<object>().Select(_documentMappers[_searchLabels[i].code].Map).ToList(),
                     TotalCount = (int)res.Total
                 }).ToList();

            /* Facette */
            var scopeFacet = new FacetOutput
            {
                Code = "FCT_SCOPE",
                Label = "Scope",
                Values = response.AllResponses.Select((dynamic res, int i) =>
                    new FacetItem
                    {
                        Code = _searchLabels[i].code,
                        Label = _searchLabels[i].label,
                        Count = (int)res.Total
                    }).ToList()
            };

            /* Construction de la sortie. */
            return new QueryOutput
            {
                Groups = groups,
                Facets = new[] { scopeFacet },
                TotalCount = response.AllResponses.Sum((dynamic res) => (int)res.Total)
            };
        }
    }

    internal interface IDocumentMapper
    {
        object Map(object input);
    }

    internal class DocumentMapper<TDocument, TOutput> : IDocumentMapper
    {
        private readonly Func<TDocument, TOutput> _mapper;

        public DocumentMapper(Func<TDocument, TOutput> mapper)
        {
            _mapper = mapper;
        }
        public object Map(object input)
        {
            return _mapper((TDocument)input);
        }
    }
}
