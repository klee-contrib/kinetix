using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Kinetix.Search.Elastic.Querying;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
    using static AdvancedQueryUtil;

    public class ElasticBulkDescriptor : ISearchBulkDescriptor
    {
        private int _operationCount = 0;

        private readonly BulkDescriptor _bulkDescriptor = new BulkDescriptor();
        private readonly ElasticClient _client;
        private readonly DocumentDescriptor _documentDescriptor;
        private readonly ILogger<ElasticStore> _logger;

        internal ElasticBulkDescriptor(DocumentDescriptor documentDescriptor, ElasticClient client, ILogger<ElasticStore> logger)
        {
            _documentDescriptor = documentDescriptor;
            _client = client;
            _logger = logger;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(string)" />
        public ISearchBulkDescriptor Delete<TDocument>(string id)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _bulkDescriptor.Delete<TDocument>(d => d.Type(def.DocumentTypeName).Id(id));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(TDocument)" />
        public ISearchBulkDescriptor Delete<TDocument>(TDocument bean)
           where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _bulkDescriptor.Delete<TDocument>(d => d.Type(def.DocumentTypeName).Id(def.PrimaryKey.GetValue(bean).ToString()));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{string})" />
        public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<string> ids)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _bulkDescriptor.DeleteMany(ids, (d, id) => d.Type(def.DocumentTypeName).Id(id));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{TDocument})" />
        public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<TDocument> beans)
           where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _bulkDescriptor.DeleteMany(
                beans.Select(bean => def.PrimaryKey.GetValue(bean).ToString()),
                (d, id) => d.Type(def.DocumentTypeName).Id(id));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Index" />
        public ISearchBulkDescriptor Index<TDocument>(TDocument document)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            var id = def.PrimaryKey.GetValue(document).ToString();

            _bulkDescriptor.Index<TDocument>(y => y
                .Document(FormatSortFields(def, document))
                .Type(def.DocumentTypeName)
                .Id(id));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.IndexMany" />
        public ISearchBulkDescriptor IndexMany<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _bulkDescriptor.IndexMany(
                documents.Select(document => FormatSortFields(def, document)),
                (b, document) => b
                    .Type(def.DocumentTypeName)
                    .Id(def.PrimaryKey.GetValue(document).ToString()));

            _operationCount++;
            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Run" />
        public void Run(bool refresh = true)
        {
            if (_operationCount > 0)
            {
                _logger.LogQuery($"Index {_operationCount}", () =>
                    _client.Bulk(_bulkDescriptor.Refresh(refresh ? Refresh.WaitFor : Refresh.False)));
            }
        }
    }
}
