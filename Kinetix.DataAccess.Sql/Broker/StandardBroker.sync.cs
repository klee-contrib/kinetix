namespace Kinetix.DataAccess.Sql.Broker;

public partial class StandardBroker<T>
{
    /// <summary>
    /// Supprime un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Clef primaire de l'objet.</param>
    public virtual void Delete(object primaryKey)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        using var tx = transactionScopeManager.EnsureTransaction();
        store.Remove(primaryKey);
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
        ArgumentNullException.ThrowIfNull(criteria);

        using var tx = transactionScopeManager.EnsureTransaction();
        store.RemoveAllByCriteria(criteria);
        tx.Complete();
    }

    /// <summary>
    /// Supprime plusieurs beans à partir de leur clé primaire.
    /// </summary>
    /// <param name="primaryKeys">Clef primaires des objets.</param>
    public virtual void DeleteCollection(ICollection<int> primaryKeys)
    {
        ArgumentNullException.ThrowIfNull(primaryKeys);

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
    public virtual T? Get(object primaryKey)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        using var tx = transactionScopeManager.EnsureTransaction();
        var bean = store.Load(primaryKey);
        tx.Complete();
        return bean;
    }

    /// <summary>
    /// Retourne tous les beans pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    public virtual IList<T> GetAll(QueryParameter? queryParameter = null)
    {
        using var tx = transactionScopeManager.EnsureTransaction();
        var coll = store.LoadAll(queryParameter);
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
    public virtual IList<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter? queryParameter = null)
    {
        using var tx = transactionScopeManager.EnsureTransaction();
        var coll = store.LoadAllByCriteria(criteria, queryParameter);
        tx.Complete();
        return coll;
    }

    /// <inheritdoc cref="IBroker{T}.GetAllByCriteria(T, QueryParameter?)" />
    public IList<T> GetAllByCriteria(T bean, QueryParameter? queryParameter = null)
    {
        return GetAllByCriteria(new FilterCriteria(bean), queryParameter);
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual T? GetByCriteria(FilterCriteria criteria)
    {
        using var tx = transactionScopeManager.EnsureTransaction();
        var value = store.LoadByCriteria(criteria);
        tx.Complete();
        return value;
    }

    /// <summary>
    /// Retourne un bean à partir d'un critère de recherche.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
    public virtual T? GetByCriteria(T criteria)
    {
        using var tx = transactionScopeManager.EnsureTransaction();
        var value = store.LoadByCriteria(new FilterCriteria(criteria));
        tx.Complete();
        return value;
    }

    /// <inheritdoc cref="IBroker{T}.Insert" />
    public object Insert(T bean, ColumnSelector? columnSelector = null)
    {
        using var tx = transactionScopeManager.EnsureTransaction();
        var result = store.Put(bean, forceInsert: true, columnSelector);
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
        ArgumentNullException.ThrowIfNull(values);

        using var tx = transactionScopeManager.EnsureTransaction();
        var result = store.PutAll(values);
        tx.Complete();
        return result;
    }

    /// <summary>
    /// Sauvegarde un bean.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou ignorer.</param>
    /// <returns>Clef primaire.</returns>
    public virtual object Save(T bean, ColumnSelector? columnSelector = null)
    {
        ArgumentNullException.ThrowIfNull(bean);

        using var tx = transactionScopeManager.EnsureTransaction();
        var primaryKey = store.Put(bean, forceInsert: false, columnSelector);
        tx.Complete();
        return primaryKey;
    }

    /// <summary>
    /// Sauvegarde l'ensemble des éléments d'une association n-n.
    /// </summary>
    /// <param name="values">Les valeurs à ajouter via associations.</param>
    /// <param name="columnSelector">Sélecteur de colonnes à mettre à jour.</param>
    /// <exception cref="ArgumentException">Si la collection n'est pas composée d'objets implémentant l'interface IBeanState.</exception>
    public virtual void SaveAll(ICollection<T> values, ColumnSelector? columnSelector = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        using var tx = transactionScopeManager.EnsureTransaction();
        foreach (var value in values)
        {
            store.Put(value, forceInsert: false, columnSelector);
        }

        tx.Complete();
    }
}
