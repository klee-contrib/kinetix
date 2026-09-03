using Kinetix.Services;

namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Broker par défaut.
/// La gestion des transactions est prise en charge par ce broker.
/// </summary>
/// <typeparam name="T">Type du bean.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="transactionScopeManager">Manager de transactions.</param>
/// <param name="store">Store.</param>
public partial class StandardBroker<T>(TransactionScopeManager transactionScopeManager, IStore<T> store) : IBroker<T>
    where T : class, new()
{
    /// <summary>
    /// Supprimé tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="bean">Critères de suppression.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task DeleteAllByCriteriaAsync(T bean, CancellationToken ct = default)
    {
        await DeleteAllByCriteriaAsync(new FilterCriteria(bean), ct);
    }

    /// <summary>
    /// Supprimé tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public virtual async Task DeleteAllByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        await store.RemoveAllByCriteriaAsync(criteria, ct);
        tx.Complete();
    }

    /// <summary>
    /// Supprime un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Clef primaire de l'objet.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public virtual async Task DeleteAsync(object primaryKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        await store.RemoveAsync(primaryKey, ct);
        tx.Complete();
    }

    /// <summary>
    /// Supprime plusieurs beans à partir de leur clé primaire.
    /// </summary>
    /// <param name="primaryKeys">Clef primaires des objets.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public virtual async Task DeleteCollectionAsync(ICollection<int> primaryKeys, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(primaryKeys);

        foreach (object primaryKey in primaryKeys)
        {
            await DeleteAsync(primaryKey, ct);
        }
    }

    /// <summary>
    /// Retourne tous les beans pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    public virtual async Task<IList<T>> GetAllAsync(
        QueryParameter? queryParameter = null,
        CancellationToken ct = default
    )
    {
        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var coll = await store.LoadAllAsync(queryParameter, ct);
        tx.Complete();
        return coll;
    }

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="criteria">Critères de sélection.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    public virtual async Task<IList<T>> GetAllByCriteriaAsync(
        FilterCriteria criteria,
        QueryParameter? queryParameter = null,
        CancellationToken ct = default
    )
    {
        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var coll = await store.LoadAllByCriteriaAsync(criteria, queryParameter, ct);
        tx.Complete();
        return coll;
    }

    /// <inheritdoc cref="IBroker{T}.GetAllByCriteriaAsync(T, QueryParameter?, CancellationToken)" />
    public Task<IList<T>> GetAllByCriteriaAsync(
        T bean,
        QueryParameter? queryParameter = null,
        CancellationToken ct = default
    )
    {
        return GetAllByCriteriaAsync(new FilterCriteria(bean), queryParameter, ct);
    }

    /// <summary>
    /// Retourne un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    public virtual async Task<T?> GetAsync(object primaryKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var bean = await store.LoadAsync(primaryKey, ct);
        tx.Complete();
        return bean;
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual async Task<T?> GetByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default)
    {
        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var value = await store.LoadByCriteriaAsync(criteria, ct);
        tx.Complete();
        return value;
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual async Task<T?> GetByCriteriaAsync(T criteria, CancellationToken ct = default)
    {
        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var value = await store.LoadByCriteriaAsync(new FilterCriteria(criteria), ct);
        tx.Complete();
        return value;
    }

    /// <summary>
    /// Insére l'ensemble des éléments.
    /// </summary>
    /// <param name="values">Valeurs à insérer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Valeurs insérées.</returns>
    public async Task<ICollection<T>> InsertAllAsync(ICollection<T> values, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var result = await store.PutAllAsync(values, ct);
        tx.Complete();
        return result;
    }

    /// <inheritdoc cref="IBroker{T}.InsertAsync" />
    public async Task<object> InsertAsync(T bean, ColumnSelector? columnSelector = null, CancellationToken ct = default)
    {
        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var result = await store.PutAsync(bean, forceInsert: true, columnSelector, ct: ct);
        tx.Complete();
        return result;
    }

    /// <summary>
    /// Sauvegarde l'ensemble des éléments d'une association n-n.
    /// </summary>
    /// <param name="values">Les valeurs à ajouter via associations.</param>
    /// <param name="columnSelector">Sélecteur de colonnes à mettre à jour.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <exception cref="ArgumentException">Si la collection n'est pas composée d'objets implémentant l'interface IBeanState.</exception>
    /// <returns>Task.</returns>
    public virtual async Task SaveAllAsync(
        ICollection<T> values,
        ColumnSelector? columnSelector = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(values);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        foreach (var value in values)
        {
            await store.PutAsync(value, forceInsert: false, columnSelector, ct: ct);
        }

        tx.Complete();
    }

    /// <summary>
    /// Sauvegarde un bean.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou ignorer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Clef primaire.</returns>
    public virtual async Task<object> SaveAsync(
        T bean,
        ColumnSelector? columnSelector = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(bean);

        await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);
        var primaryKey = await store.PutAsync(bean, forceInsert: false, columnSelector, ct: ct);
        tx.Complete();
        return primaryKey;
    }
}
