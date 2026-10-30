using Elasticsearch.Net;
using Kinetix.Search.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Core.DocumentModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

public class ElasticBulkDescriptor : ISearchBulkDescriptor
{
    private readonly BulkDescriptor _bulkDescriptor = new BulkDescriptor()
        .Timeout(TimeSpan.FromMinutes(1))
        .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(1)));
    private readonly ElasticClient _client;
    private readonly SearchConfig _config;
    private readonly DocumentDescriptor _documentDescriptor;
    private readonly Dictionary<Type, int> _indexTypes = [];
    private readonly ILogger<ElasticStore> _logger;

    internal ElasticBulkDescriptor(
        SearchConfig config,
        DocumentDescriptor documentDescriptor,
        ElasticClient client,
        ILogger<ElasticStore> logger
    )
    {
        _client = client;
        _config = config;
        _documentDescriptor = documentDescriptor;
        _logger = logger;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}" />
    public ISearchBulkDescriptor Delete<TDocument>(object key)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.Delete<TDocument>(o => o.Id(def.PrimaryKey.GetValueFromKeyObject(key)));
        AddOperationCount<TDocument>(1);

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.DeleteMany{TDocument}" />
    public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<object> keys)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var values = keys.Select(def.PrimaryKey.GetValueFromKeyObject).ToList();
        _bulkDescriptor.DeleteMany<TDocument>(values);
        AddOperationCount<TDocument>(values.Count);
        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Index{TDocument}" />
    public ISearchBulkDescriptor Index<TDocument>(TDocument document)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var id = def.PrimaryKey.GetValueFromDocument(document);
        _bulkDescriptor.Index<TDocument>(y => y.Document(document).Id(id));
        AddOperationCount<TDocument>(1);

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.IndexMany{TDocument}" />
    public ISearchBulkDescriptor IndexMany<TDocument>(IList<TDocument> documents)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.IndexMany(documents, (b, document) => b.Id(def.PrimaryKey.GetValueFromDocument(document)));
        AddOperationCount<TDocument>(documents.Count);
        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Run" />
    public void Run(bool refresh = true)
    {
        if (_indexTypes.Count > 0)
        {
            _logger.LogQuery(
                _config,
                $"Index: {string.Join(", ", _indexTypes.Select(kv => $"{kv.Value} {SearchConfig.GetTypeNameForIndex(kv.Key)}"))}",
                indexType: null,
                () => _client.Bulk(_bulkDescriptor.Refresh(refresh ? Refresh.WaitFor : Refresh.False))
            );
        }
    }

    private void AddOperationCount<TDocument>(int count)
        where TDocument : class
    {
        var type = typeof(TDocument);
        if (_indexTypes.ContainsKey(type))
        {
            _indexTypes[type] += count;
        }
        else
        {
            _indexTypes[type] = count;
        }
    }
}
