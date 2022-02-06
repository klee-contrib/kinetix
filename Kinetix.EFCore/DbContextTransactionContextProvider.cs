using Kinetix.Services;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.EFCore;

internal class DbContextTransactionContextProvider<TDbContext> : ITransactionContextProvider
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public DbContextTransactionContextProvider(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    /// <inheritdoc cref="ITransactionContext.Create" />
    public ITransactionContext Create()
    {
        return new DbContextTransactionContext(_dbContext);
    }
}
