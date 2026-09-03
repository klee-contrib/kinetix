namespace Kinetix.DataAccess.Sql.Broker;

public partial interface IStore<T>
{
    /// <summary>
    /// Charge un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <returns>Bean.</returns>
    T? Load(object primaryKey);

    /// <summary>
    /// Charge toutes les données pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    IList<T> LoadAll(QueryParameter? queryParameter);

    /// <summary>
    /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
    /// </summary>
    /// <param name="criteria">Map de critères auquelle la recherche doit correpondre.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    IList<T> LoadAllByCriteria(FilterCriteria criteria, QueryParameter? queryParameter);

    /// <summary>
    /// Récupération d'un objet à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Objet.</returns>
    T? LoadByCriteria(FilterCriteria criteria);

    /// <summary>
    /// Dépose un bean dans le store.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="forceInsert">Force un insert (au lieu de déterminer automatiquement en fonction de la PK).</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    object Put(T bean, bool forceInsert, ColumnSelector? columnSelector = null);

    /// <summary>
    /// Dépose les beans dans le store.
    /// </summary>
    /// <param name="collection">Beans à enregistrer.</param>
    /// <returns>Beans enregistrés.</returns>
    ICollection<T> PutAll(ICollection<T> collection);

    /// <summary>
    /// Supprime un bean du store.
    /// </summary>
    /// <param name="primaryKey">Clef primaire du bean à supprimer.</param>
    void Remove(object primaryKey);

    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    void RemoveAllByCriteria(FilterCriteria criteria);
}
