using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Configurateur des indexes ES par défaut.
    /// </summary>
    public class DefaultIndexConfigurator : IIndexConfigurator
    {
        /// <inheritdoc cref="IIndexConfigurator.ConfigureIndex" />
        public ICreateIndexRequest ConfigureIndex(CreateIndexDescriptor descriptor)
        {
            return descriptor.Settings(s => s
                .Analysis(a => a
                    .CharFilters(c => c
                        .PatternReplace("unsignificant", p => p
                            .Pattern("[\\.()]")
                            .Replacement(string.Empty))
                        .PatternReplace("start", p => p
                            .Pattern("^[- ']+")
                            .Replacement(string.Empty))
                        .PatternReplace("end", p => p
                            .Pattern("[- ']+$")
                            .Replacement(string.Empty))
                        .PatternReplace("spaces", p => p
                            .Pattern("[- ']+")
                            .Replacement(" ")))
                    .TokenFilters(t => t
                        .EdgeNGram("edgengram", e => e
                            .MinGram(1)
                            .MaxGram(50)))
                    .Tokenizers(t => t
                        .CharGroup("chargroup", c => c
                            .TokenizeOnCharacters(" ", "-", "'")))
                    .Analyzers(a => a
                        .Custom("text", c => c
                            .Tokenizer("chargroup")
                            .Filters("edgengram", "asciifolding", "lowercase"))
                        .Custom("search_text", c => c
                            .Tokenizer("chargroup")
                            .Filters("asciifolding", "lowercase")))
                    .Normalizers(n => n
                        .Custom("keyword", c => c
                            .CharFilters("unsignificant", "start", "end", "spaces")
                            .Filters("asciifolding", "lowercase"))))
                .Setting("index.translog.retention.age", "30m")
                .Setting("index.translog.retention.size", "64mb"));
        }
    }
}