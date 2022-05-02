using Kinetix.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.EFCore;

/// <summary>
/// Extension du DbContaxt pour detacher les objets dont on a déjà save les modifications
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Enregistre EFCore avec la gestion de transactions Kinetix.
    /// </summary>
    /// <typeparam name="TDbContext">Type du DBContext.</typeparam>
    /// <param name="services">ServiceCollection.</param>
    /// <param name="optionsAction">Configuration.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddEFCore<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction = null)
        where TDbContext : DbContext
    {
        return services
            .AddDbContext<TDbContext>(optionsAction)
            .AddScoped<ITransactionContextProvider, DbContextTransactionContextProvider<TDbContext>>();
    }

    /// <summary>
    /// Détache toutes les entités déjà traitées
    /// </summary>
    /// <param name="context">Context</param>
    public static void DetachAll(this DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity != null)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
