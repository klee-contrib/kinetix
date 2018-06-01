using System.Collections.Generic;
using Kinetix.Monitoring.Abstractions;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Contract;
using Kinetix.Search.Model;

namespace Kinetix.Search
{
    /// <summary>
    /// Implémentation standard des brokers de recherche.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class SearchBroker<TDocument> : ISearchBroker<TDocument>
    {
        /// <summary>
        /// Nombre de résultat par défaut pour une query.
        /// </summary>
        private const int QueryDefaultSize = 10;

        private ISearchStore<TDocument> _store;

        private IAnalyticsManager _analyticsManager;

        public static readonly string CATEGORY = "search";

        public SearchBroker(ISearchStore<TDocument> store, IAnalyticsManager analyticsManager)
        {
            _store = store;
            _analyticsManager = analyticsManager;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.CreateDocumentType" />
        public void CreateDocumentType()
        {
            _store.CreateDocumentType();
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Get" />
        public TDocument Get(string id)
        {
            TDocument ret = default(TDocument);
            _analyticsManager.Trace(CATEGORY, "/Get/" + _store.IndexName, tracer =>
            {
                ret = _store.Get(id);
            });
            return ret;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Put" />
        public void Put(TDocument document)
        {
            _analyticsManager.Trace(CATEGORY, "/Put/" + _store.IndexName, tracer =>
            {
                _store.Put(document);
            });
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList)
        {
            _analyticsManager.Trace(CATEGORY, "/PutAll/" + _store.IndexName, tracer =>
            {
                _store.PutAll(documentList);
            });
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Remove" />
        public void Remove(string id)
        {
            _analyticsManager.Trace(CATEGORY, "/Remove/" + _store.IndexName, tracer =>
            {
                _store.Remove(id);
            });


        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Flush" />
        public void Flush()
        {
            _analyticsManager.Trace(CATEGORY, "/Flush/" + _store.IndexName, tracer =>
            {
                _store.Flush();
            });

        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Query" />
        public IEnumerable<TDocument> Query(string text, string security = null)
        {
            ICollection<TDocument> results = _analyticsManager.TraceWithReturn(CATEGORY, "/Query/" + _store.IndexName, tracer =>
            {
                if (string.IsNullOrEmpty(text))
                {
                    return new List<TDocument>();
                }

                var input = new AdvancedQueryInput
                {
                    ApiInput = new QueryInput
                    {
                        Criteria = new Criteria
                        {
                            Query = text
                        },
                        Skip = 0,
                        Top = QueryDefaultSize
                    },
                    Security = security
                };
                var output = _store.AdvancedQuery(input);
                return output.List;

            });

            return results;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedQuery" />
        public QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input)
        {
            QueryOutput<TDocument> results = _analyticsManager.TraceWithReturn(CATEGORY, "/AdvancedQuery/" + _store.IndexName, tracer =>
            {
                return _store.AdvancedQuery(input);
            });

            return results;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedCount" />
        public long AdvancedCount(AdvancedQueryInput input)
        {

            long count = _analyticsManager.TraceWithReturn(CATEGORY, "/AdvancedCount/" + _store.IndexName, tracer =>
            {
                return _store.AdvancedCount(input);
            });

            return count;
        }
    }
}
