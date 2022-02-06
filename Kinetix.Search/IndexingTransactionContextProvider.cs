using Kinetix.Services;

namespace Kinetix.Search;

internal class IndexingTransactionContextProvider : ITransactionContextProvider
{
    private readonly IServiceProvider _provider;

    public IndexingTransactionContextProvider(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ITransactionContext Create()
    {
        return new IndexingTransactionContext(_provider);
    }
}
