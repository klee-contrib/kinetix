using Kinetix.Services;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Fournisseur de contexte de transaction pour les connections en BDD.
/// </summary>
public class SqlTransactionContextProvider : ITransactionContextProvider
{
    /// <inheritdoc cref="ITransactionContextProvider.Create" />
    public ITransactionContext Create()
    {
        return new SqlTransactionContext();
    }
}
