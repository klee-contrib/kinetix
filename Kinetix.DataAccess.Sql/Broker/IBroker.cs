namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Interface permettant de manipuler les brokers sans leur type anonyme.
/// </summary>
public interface IBroker { }

/// <summary>
/// Interface de définition d'un broker d'accès aux données.
/// </summary>
/// <typeparam name="T">Type du bean à manipuler.</typeparam>
public partial interface IBroker<T> : IBroker
    where T : new()
{
    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task DeleteAllByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="bean">Critères de suppression.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task DeleteAllByCriteriaAsync(T bean, CancellationToken ct = default);

    /// <summary>
    /// Supprime un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Clef primaire de l'objet.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task DeleteAsync(object primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Supprime plusieurs beans à partir de leur clé primaire.
    /// </summary>
    /// <param name="primaryKeys">Clef primaires des objets.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task DeleteCollectionAsync(ICollection<int> primaryKeys, CancellationToken ct = default);

    /// <summary>
    /// Retourne tous les beans pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    Task<IList<T>> GetAllAsync(QueryParameter? queryParameter = null, CancellationToken ct = default);

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="criteria">Liste des critères.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    Task<IList<T>> GetAllByCriteriaAsync(
        FilterCriteria criteria,
        QueryParameter? queryParameter = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="bean">Bean de critère.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    Task<IList<T>> GetAllByCriteriaAsync(T bean, QueryParameter? queryParameter = null, CancellationToken ct = default);

    /// <summary>
    /// Retourne un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    Task<T?> GetAsync(object primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Retourne un bean à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    Task<T?> GetByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Retourne un bean à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    Task<T?> GetByCriteriaAsync(T criteria, CancellationToken ct = default);

    /// <summary>
    /// Insére l'ensemble des éléments.
    /// </summary>
    /// <param name="values">Valeurs à insérer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Valeurs insérées.</returns>
    Task<ICollection<T>> InsertAllAsync(ICollection<T> values, CancellationToken ct = default);

    /// <summary>
    /// Insère un élément.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    Task<object> InsertAsync(T bean, ColumnSelector? columnSelector = null, CancellationToken ct = default);

    /// <summary>
    /// Sauvegarde l'ensemble des éléments d'une association n-n.
    /// </summary>
    /// <param name="values">Les valeurs à ajouter via associations.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task SaveAllAsync(ICollection<T> values, ColumnSelector? columnSelector = null, CancellationToken ct = default);

    /// <summary>
    /// Sauvegarde un bean (update si PK renseignée, insert sinon).
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    Task<object> SaveAsync(T bean, ColumnSelector? columnSelector = null, CancellationToken ct = default);
}
