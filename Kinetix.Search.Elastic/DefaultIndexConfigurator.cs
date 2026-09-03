using Elastic.Clients.Elasticsearch.IndexManagement;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Configurateur des indexes ES par défaut.
/// </summary>
public class DefaultIndexConfigurator : IIndexConfigurator
{
    /// <inheritdoc cref="IIndexConfigurator.ConfigureIndex" />
    public void ConfigureIndex(CreateIndexRequestDescriptor descriptor)
    {
        descriptor.Settings(s =>
            s.Analysis(a =>
                a.CharFilters(c =>
                        c.PatternReplace("unsignificant", p => p.Pattern("[\\.()]").Replacement(string.Empty))
                            .PatternReplace("start", p => p.Pattern("^[- ']+").Replacement(string.Empty))
                            .PatternReplace("end", p => p.Pattern("[- ']+$").Replacement(string.Empty))
                            .PatternReplace("spaces", p => p.Pattern("[- ']+").Replacement(" "))
                    )
                    .TokenFilters(t => t.EdgeNGram("edgengram", e => e.MinGram(1).MaxGram(50)))
                    .Tokenizers(t => t.CharGroup("chargroup", c => c.TokenizeOnChars(" ", "-", "'")))
                    .Analyzers(a =>
                        a.Custom("text", c => c.Tokenizer("chargroup").Filter("edgengram", "asciifolding", "lowercase"))
                            .Custom("search_text", c => c.Tokenizer("chargroup").Filter("asciifolding", "lowercase"))
                    )
                    .Normalizers(n =>
                        n.Custom(
                            "keyword",
                            c =>
                                c.CharFilter("unsignificant", "start", "end", "spaces")
                                    .Filter("asciifolding", "lowercase")
                        )
                    )
            )
        );
    }
}
