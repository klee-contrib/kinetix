using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Configurateur des indexes ES par défaut.
    /// </summary>
    public class DefaultIndexConfigurator : IIndexConfigurator
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        public DefaultIndexConfigurator()
        {
            IndexSettings = new IndexSettings
            {
                Analysis = new Analysis
                {
                    TokenFilters = new TokenFilters
                    {
                        ["autocomplete"] = new EdgeNGramTokenFilter { MinGram = 1, MaxGram = 20 }
                    },
                    Analyzers = new Analyzers
                    {
                        ["text"] = new CustomAnalyzer
                        {
                            Tokenizer = "whitespace",
                            Filter = new[] { "autocomplete", "asciifolding", "lowercase" }
                        },
                        ["search_text"] = new CustomAnalyzer
                        {
                            Tokenizer = "whitespace",
                            Filter = new[] { "asciifolding", "lowercase" }
                        }
                    }
                }
            };

            Configure();
        }

        /// <inheritdoc />
        public IIndexSettings IndexSettings { get; }

        /// <inheritdoc />
        public ICreateIndexRequest CreateIndexRequest(IndexName indexName)
        {
            return new CreateIndexRequest(indexName) { Settings = IndexSettings };
        }

        /// <summary>
        /// Pour étendre/modifier la configuration de base.
        /// </summary>
        public virtual void Configure()
        {
        }
    }
}