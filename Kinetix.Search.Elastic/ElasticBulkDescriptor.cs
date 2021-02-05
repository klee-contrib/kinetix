﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
    public class ElasticBulkDescriptor : ISearchBulkDescriptor
    {
        private int _operationCount = 0;

        private readonly SearchAnalytics _analytics;
        private readonly BulkDescriptor _bulkDescriptor = new BulkDescriptor()
            .Timeout(TimeSpan.FromMinutes(1))
            .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(1)));
        private readonly ElasticClient _client;
        private readonly DocumentDescriptor _documentDescriptor;
        private readonly ILogger<ElasticStore> _logger;

        internal ElasticBulkDescriptor(DocumentDescriptor documentDescriptor, ElasticClient client, ILogger<ElasticStore> logger, SearchAnalytics analytics)
        {
            _analytics = analytics;
            _documentDescriptor = documentDescriptor;
            _client = client;
            _logger = logger;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(string)" />
        public ISearchBulkDescriptor Delete<TDocument>(string id)
            where TDocument : class
        {
            _bulkDescriptor.Delete<TDocument>(d => d.Id(id));
            _operationCount++;
            _analytics.CountDelete(1);

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(TDocument)" />
        public ISearchBulkDescriptor Delete<TDocument>(TDocument bean)
           where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            _bulkDescriptor.Delete<TDocument>(d => d.Id(def.PrimaryKey.GetValue(bean).ToString()));
            _operationCount++;
            _analytics.CountDelete(1);

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{string})" />
        public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<string> ids)
            where TDocument : class
        {
            if (ids.Any())
            {
                _bulkDescriptor.DeleteMany<TDocument>(ids, (d, id) => d.Id(id));
                _operationCount++;
                _analytics.CountDelete(ids.Count());
            }

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{TDocument})" />
        public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<TDocument> beans)
           where TDocument : class
        {
            if (beans.Any())
            {
                var def = _documentDescriptor.GetDefinition(typeof(TDocument));
                _bulkDescriptor.DeleteMany<TDocument>(
                    beans.Select(bean => def.PrimaryKey.GetValue(bean).ToString()),
                    (d, id) => d.Id(id));
                _operationCount++;
                _analytics.CountDelete(beans.Count());
            }

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Index" />
        public ISearchBulkDescriptor Index<TDocument>(TDocument document)
            where TDocument : class
        {
            if (document != null)
            {
                var def = _documentDescriptor.GetDefinition(typeof(TDocument));
                var id = def.PrimaryKey.GetValue(document).ToString();
                _bulkDescriptor.Index<TDocument>(y => y.Document(document).Id(id));
                _operationCount++;
                _analytics.CountIndex(1);
            }

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.IndexMany" />
        public ISearchBulkDescriptor IndexMany<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : class
        {
            if (documents.Any())
            {
                var def = _documentDescriptor.GetDefinition(typeof(TDocument));
                _bulkDescriptor.IndexMany(
                    documents,
                    (b, document) => b.Id(def.PrimaryKey.GetValue(document).ToString()));
                _operationCount++;
                _analytics.CountIndex(documents.Count());
            }

            return this;
        }

        /// <inheritdoc cref="ISearchBulkDescriptor.Run" />
        public int Run(bool refresh = true)
        {
            if (_operationCount > 0)
            {
                _logger.LogQuery(_analytics, $"Index {_operationCount}", () =>
                     _client.Bulk(_bulkDescriptor.Refresh(refresh ? Refresh.WaitFor : Refresh.False)));
            }

            return _operationCount;
        }
    }
}