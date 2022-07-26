using Kinetix.Services;

namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Broker par défaut.
/// La gestion des transactions est prise en charge par ce broker.
/// </summary>
/// <typeparam name="T">Type du bean.</typeparam>
public class StandardBroker<T> : IBroker<T>
    where T : class, new()
{
    private readonly IStore<T> _store;
    private readonly TransactionScopeManager _transactionScopeManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="transactionScopeManager">Manager de transactions.</param>
    /// <param name="store">Store.</param>
    public StandardBroker(TransactionScopeManager transactionScopeManager, IStore<T> store)
    {
        _store = store;
        _transactionScopeManager = transactionScopeManager;
    }

    /// <summary>
    /// Vérifie si au moins un objet dans la collection est utilisé.
    /// </summary>
    /// <param name="primaryKeys">Clés primaires des objets à vérifier.</param>
    /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
    /// <returns>True si au moins un objet est utilisé.</returns>
    public bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null)
    {
        return _store.AreUsed(primaryKeys, tablesToIgnore);
    }

    /// <summary>
    /// Supprime un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Clef primaire de l'objet.</param>
    public virtual void Delete(object primaryKey)
    {
        if (primaryKey == null)
        {
            throw new ArgumentNullException(nameof(primaryKey));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        _store.Remove(primaryKey);
        tx.Complete();
    }

    /// <summary>
    /// Supprimé tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="bean">Critères de suppression.</param>
    public void DeleteAllByCriteria(T bean)
    {
        DeleteAllByCriteria(new FilterCriteria(bean));
    }

    /// <summary>
    /// Supprimé tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    public virtual void DeleteAllByCriteria(FilterCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        _store.RemoveAllByCriteria(criteria);
        tx.Complete();
    }

    /// <summary>
    /// Supprime plusieurs beans à partir de leur clé primaire.
    /// </summary>
    /// <param name="primaryKeys">Clef primaires des objets.</param>
    public virtual void DeleteCollection(ICollection<int> primaryKeys)
    {
        if (primaryKeys == null)
        {
            throw new ArgumentNullException(nameof(primaryKeys));
        }

        foreach (object primaryKey in primaryKeys)
        {
            Delete(primaryKey);
        }
    }

    /// <summary>
    /// Retourne un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <returns>Bean.</returns>
    public virtual T Get(object primaryKey)
    {
        if (primaryKey == null)
        {
            throw new ArgumentNullException(nameof(primaryKey));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        var bean = _store.Load(primaryKey);
        tx.Complete();
        return bean;
    }

    /// <summary>
    /// Retourne tous les beans pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    public virtual IList<T> GetAll(QueryParameter queryParameter = null)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();
        var coll = _store.LoadAll(queryParameter);
        tx.Complete();
        return coll;
    }

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="criteria">Critères de sélection.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    public virtual IList<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter queryParameter = null)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();
        var coll = _store.LoadAllByCriteria(criteria, queryParameter);
        tx.Complete();
        return coll;
    }

    /// <inheritdoc cref="IBroker{T}.GetAllByCriteria(T, QueryParameter)" />
    public IList<T> GetAllByCriteria(T bean, QueryParameter queryParameter = null)
    {
        return GetAllByCriteria(new FilterCriteria(bean), queryParameter);
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual T GetByCriteria(FilterCriteria criteria)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();
        var value = _store.LoadByCriteria(criteria);
        tx.Complete();
        return value;
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual T GetByCriteria(T criteria)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();
        var value = _store.LoadByCriteria(new FilterCriteria(criteria));
        tx.Complete();
        return value;
    }

    public object Insert(T bean, ColumnSelector columnSelector = null)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();
        var result = _store.Put(bean, forceInsert: true, columnSelector);
        tx.Complete();
        return result;
    }

    /// <summary>
    /// Insére l'ensemble des éléments.
    /// </summary>
    /// <param name="values">Valeurs à insérer.</param>
    /// <returns>Valeurs insérées.</returns>
    public ICollection<T> InsertAll(ICollection<T> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        var result = _store.PutAll(values);
        tx.Complete();
        return result;
    }

    /// <summary>
    /// Vérifie si l'objet est utilisé.
    /// </summary>
    /// <param name="primaryKey">Clé primaire de l'objet à vérifier.</param>
    /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
    /// <returns>True si l'objet est utilisé.</returns>
    public bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null)
    {
        return _store.IsUsed(primaryKey, tablesToIgnore);
    }

    /// <summary>
    /// Sauvegarde un bean.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou ignorer.</param>
    /// <returns>Clef primaire.</returns>
    public virtual object Save(T bean, ColumnSelector columnSelector)
    {
        if (bean == null)
        {
            throw new ArgumentNullException(nameof(bean));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        var primaryKey = _store.Put(bean, forceInsert: false, columnSelector);
        tx.Complete();
        return primaryKey;
    }

    /// <summary>
    /// Sauvegarde l'ensemble des éléments d'une association n-n.
    /// </summary>
    /// <param name="values">Les valeurs à ajouter via associations.</param>
    /// <param name="columnSelector">Sélecteur de colonnes à mettre à jour.</param>
    /// <exception cref="ArgumentException">Si la collection n'est pas composée d'objets implémentant l'interface IBeanState.</exception>
    public virtual void SaveAll(ICollection<T> values, ColumnSelector columnSelector = null)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        using var tx = _transactionScopeManager.EnsureTransaction();
        foreach (var value in values)
        {
            _store.Put(value, forceInsert: false, columnSelector);
        }

        tx.Complete();
    }
}
