using Kinetix.Services;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.EFCore;

internal class DbContextTransactionContextProvider<TDbContext>(TDbContext dbContext) : ITransactionContextProvider
    where TDbContext : DbContext
{
    /// <inheritdoc cref="ITransactionContextProvider.Create" />
    public ITransactionContext Create()
    {
        return new DbContextTransactionContext(dbContext);
    }
}
