using Elasticsearch.Net;
using Kinetix.Monitoring.Core;
using Kinetix.Search.Core;
using Kinetix.Search.Core.DocumentModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

public class ElasticBulkDescriptor : ISearchBulkDescriptor
{
    private readonly AnalyticsManager _analytics;
    private readonly BulkDescriptor _bulkDescriptor = new BulkDescriptor()
        .Timeout(TimeSpan.FromMinutes(1))
        .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(1)));

    private readonly ElasticClient _client;
    private readonly DocumentDescriptor _documentDescriptor;
    private readonly ILogger<ElasticStore> _logger;
    private int _operationCount = 0;

    internal ElasticBulkDescriptor(
        DocumentDescriptor documentDescriptor,
        ElasticClient client,
        ILogger<ElasticStore> logger,
        AnalyticsManager analytics
    )
    {
        _analytics = analytics;
        _client = client;
        _documentDescriptor = documentDescriptor;
        _logger = logger;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}" />
    public ISearchBulkDescriptor Delete<TDocument>(object key)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.Delete<TDocument>(o => o.Id(def.PrimaryKey.GetValueFromKeyObject(key)));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.DeleteMany{TDocument}" />
    public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<object> keys)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.DeleteMany<TDocument>(keys.Select(def.PrimaryKey.GetValueFromKeyObject));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Index{TDocument}" />
    public ISearchBulkDescriptor Index<TDocument>(TDocument document)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var id = def.PrimaryKey.GetValueFromDocument(document);
        _bulkDescriptor.Index<TDocument>(y => y.Document(document).Id(id));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.IndexMany{TDocument}" />
    public ISearchBulkDescriptor IndexMany<TDocument>(IList<TDocument> documents)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.IndexMany(documents, (b, document) => b.Id(def.PrimaryKey.GetValueFromDocument(document)));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.RunAsync" />
    public async Task<int> RunAsync(bool refresh = true, CancellationToken ct = default)
    {
        if (_operationCount > 0)
        {
            await _logger.LogQueryAsync(
                _analytics,
                $"Index {_operationCount}",
                (ct) => _client.BulkAsync(_bulkDescriptor.Refresh(refresh ? Refresh.WaitFor : Refresh.False), ct),
                ct
            );
        }

        return _operationCount;
    }
}
