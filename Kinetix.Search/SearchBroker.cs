using System.Collections.Generic;
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
        private ISearchStore<TDocument> _store;

        public SearchBroker(ISearchStore<TDocument> store)
        {
            _store = store;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.CreateDocumentType" />
        public void CreateDocumentType()
        {
            _store.CreateDocumentType();
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Get" />
        public TDocument Get(string id)
        {
            return _store.Get(id);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Put" />
        public void Put(TDocument document)
        {
            _store.Put(document);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList)
        {
            _store.PutAll(documentList);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Remove" />
        public void Remove(string id)
        {
            _store.Remove(id);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Flush" />
        public void Flush()
        {
            _store.Flush();
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Query" />
        public (IEnumerable<TDocument> data, int totalCount) Query(string text, string security = null, int top = 10)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (new List<TDocument>(), 0);
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
                    Top = top
                },
                Security = security
            };
            var output = _store.AdvancedQuery(input);
            return (output.List, output.TotalCount.Value);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedQuery" />
        public QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input)
        {
            return _store.AdvancedQuery(input);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedCount" />
        public long AdvancedCount(AdvancedQueryInput input)
        {
            return _store.AdvancedCount(input);
        }
    }
}
