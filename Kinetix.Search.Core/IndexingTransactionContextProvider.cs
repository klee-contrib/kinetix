using Kinetix.Services;

namespace Kinetix.Search.Core;

internal class IndexingTransactionContextProvider(IServiceProvider provider) : ITransactionContextProvider
{
    /// <inheritdoc cref="ITransactionContextProvider.Create" />
    public ITransactionContext Create()
    {
        return new IndexingTransactionContext(provider);
    }
}
