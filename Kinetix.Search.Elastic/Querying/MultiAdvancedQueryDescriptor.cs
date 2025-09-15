#pragma warning disable MA0048

using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Nest;

namespace Kinetix.Search.Elastic.Querying;

using static AdvancedQueryUtil;

public class MultiAdvancedQueryDescriptor(
    ElasticClient client,
    DocumentDescriptor documentDescriptor,
    FacetHandler facetHandler
) : IMultiAdvancedQueryDescriptor
{
    private readonly Dictionary<string, IDocumentMapper> _documentMappers = [];
    private readonly Dictionary<string, ISearchRequest> _searchDescriptors = [];
    private readonly List<(string Code, string Label)> _searchLabels = [];

    /// <inheritdoc cref="IMultiAdvancedQueryDescriptor.AddQuery{TDocument, TOutput, TCriteria}(string, string, AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, TOutput})" />
    public IMultiAdvancedQueryDescriptor AddQuery<TDocument, TOutput, TCriteria>(
        string code,
        string label,
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, TOutput> documentMapper
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return AddQuery(code, label, input, (d, _) => documentMapper(d));
    }

    /// <inheritdoc cref="IMultiAdvancedQueryDescriptor.AddQuery{TDocument, TOutput, TCriteria}(string, string, AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, IReadOnlyDictionary{string, IReadOnlyCollection{string}}, TOutput})" />
    public IMultiAdvancedQueryDescriptor AddQuery<TDocument, TOutput, TCriteria>(
        string code,
        string label,
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        foreach (var sc in input.SearchCriteria)
        {
            sc.Group = null;
        }

        var def = documentDescriptor.GetDefinition(typeof(TDocument));
        _searchDescriptors.Add(
            code,
            GetAdvancedQueryDescriptor(
                def,
                input,
                facetHandler,
                filter: null,
                sorts: null,
                sortsAfter: false,
                aggs: null,
                input.FacetQueryDefinition.Facets,
                GetGroupFieldName(input)
            )(new SearchDescriptor<TDocument>())
        );
        _documentMappers.Add(code, new DocumentMapper<TDocument, TOutput>(documentMapper));
        _searchLabels.Add((code, label));
        return this;
    }

    /// <inheritdoc cref="IMultiAdvancedQueryDescriptor.Search" />
    public QueryOutput Search()
    {
        var response = client.MultiSearch(new MultiSearchRequest { Operations = _searchDescriptors });

        /* Extraction des résultats. */
        var groups = response
            .AllResponses.Select(
                (dynamic res, int i) =>
                    new GroupResult
                    {
                        Code = _searchLabels[i].Code,
                        Label = _searchLabels[i].Label,
                        List = ((IEnumerable<dynamic>)res.Hits)
                            .Select(h => _documentMappers[_searchLabels[i].Code].Map(h.Source, h.Highlight))
                            .ToList(),
                        TotalCount = (int)res.Total,
                    }
            )
            .ToList();

        /* Facette */
        var scopeFacet = new FacetOutput
        {
            Code = "FCT_SCOPE",
            Label = "Scope",
            Values = response
                .AllResponses.Select(
                    (dynamic res, int i) =>
                        new FacetItem
                        {
                            Code = _searchLabels[i].Code,
                            Label = _searchLabels[i].Label,
                            Count = (int)res.Total,
                        }
                )
                .ToList(),
        };

        /* Construction de la sortie. */
        return new QueryOutput
        {
            Groups = groups,
            Facets = [scopeFacet],
            TotalCount = response.AllResponses.Sum((dynamic res) => (int)res.Total),
        };
    }
}

internal interface IDocumentMapper
{
    object Map(object input, IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlights);
}

internal class DocumentMapper<TDocument, TOutput>(
    Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> mapper
) : IDocumentMapper
{
    /// <inheritdoc cref="IDocumentMapper.Map" />
    public object Map(object input, IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlights)
    {
        return mapper((TDocument)input, highlights)!;
    }
}
