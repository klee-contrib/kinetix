using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

/// <summary>
/// Manager de transactions.
/// </summary>
public class TransactionScopeManager(
    IEnumerable<ITransactionContextProvider> contextProviders,
    ILogger<ServiceScope> logger
) : IAsyncDisposable, IDisposable
{
    private readonly Stack<ServiceScope> _scopes = new();

    /// <summary>
    /// Scope de transaction actif, avec ses contextes transactionnels.
    /// </summary>
    /// <remarks>(Un sous-scope créé par <see cref="EnsureTransaction"/> ou <see cref="EnsureTransactionAsync(CancellationToken)"/> n'est pas le scope actif)</remarks>
    public ServiceScope? ActiveScope => _scopes.Any() ? _scopes.Peek() : null;

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
    /// Débute une nouvelle transaction synchrone, indépendante d'une éventuelle transaction existante.
    /// <br />
    /// <br />
    /// Cette transaction gérera ces propres contextes, comme ses connections en BDD par exemple.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Scope de la transaction.</returns>
    public async Task<ServiceScope> BeginNewTransactionAsync(CancellationToken ct = default)
    {
        var scope = CreateScope();
        await scope.InitAsync(ct);
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
    /// Libére tous les scopes de transactions (s'ils n'ont pas déjà été libérés).
    /// </summary>
    /// <returns>Task.</returns>
    public async ValueTask DisposeAsync()
    {
        foreach (var scope in _scopes.ToList())
        {
            await scope.DisposeAsync();
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

    /// <summary>
    /// Vérifie la présence d'une transaction pré-existante, et la crée le cas échéant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Scope de la transaction (actif si la transaction a été créee).</returns>
    public async Task<ServiceScope> EnsureTransactionAsync(CancellationToken ct = default)
    {
        return ActiveScope != null ? new ServiceScope() : await BeginNewTransactionAsync(ct);
    }

    /// <summary>
    /// Retire le scope demandé de la pile de scope, s'il s'agit bien de lui.
    /// </summary>
    /// <param name="scope">Scope de transaction.</param>
    internal void PopScope(ServiceScope scope)
    {
        var activeScope = _scopes.Pop();
        if (activeScope != scope)
        {
            throw new InvalidOperationException("Erreur lors de la clôture d'une transaction");
        }
    }

    private ServiceScope CreateScope()
    {
        var scope = new ServiceScope(contextProviders.Select(ctx => ctx.Create()).ToArray(), logger, this);
        _scopes.Push(scope);
        return scope;
    }
}
