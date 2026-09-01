namespace Kinetix.Services;

public partial class TransactionScopeManager : IDisposable
{
    /// <summary>
    /// Débute une nouvelle transaction synchrone, indépendante d'une éventuelle transaction existante.
    /// <br />
    /// <br />
    /// Cette transaction gérera ces propres contextes, comme ses connections en BDD par exemple.
    /// </summary>
    /// <remarks>Cette transaction ne pourra gérer que des contextes transactionnels synchrones, et devra être libérée avec <see cref="Dispose"/>. Si vous souhaitez gérer des contextes transactionnels asynchrones, utilisez <see cref="BeginNewTransactionAsync"/> à la place.</remarks>
    /// <returns>Scope de la transaction.</returns>
    public ServiceScope BeginNewTransaction()
    {
        var scope = CreateScope();
        scope.Init();
        return scope;
    }

    /// <summary>
    /// Libére tous les scopes de transactions (s'ils n'ont pas déjà été libérés).
    /// </summary>
    public void Dispose()
    {
        foreach (var scope in _scopes.ToList())
        {
            scope.Dispose();
        }
    }

    /// <summary>
    /// Vérifie la présence d'une transaction pré-existante, et la crée le cas échéant.
    /// </summary>
    /// <remarks>Cette transaction ne pourra gérer que des contextes transactionnels synchrones, et devra être libérée avec <see cref="Dispose"/>. Si vous souhaitez gérer des contextes transactionnels asynchrones, utilisez <see cref="EnsureTransactionAsync(CancellationToken)"/> à la place.</remarks>
    /// <returns>Scope de la transaction (actif si la transaction a été créee).</returns>
    public ServiceScope EnsureTransaction()
    {
        return ActiveScope != null ? new ServiceScope() : BeginNewTransaction();
    }
}
