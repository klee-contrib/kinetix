namespace Kinetix.DataAccess.Sql.Broker;

public partial interface IBroker<T>
{
    /// <summary>
    /// Supprime un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Clef primaire de l'objet.</param>
    void Delete(object primaryKey);

    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    void DeleteAllByCriteria(FilterCriteria criteria);

    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="bean">Critères de suppression.</param>
    void DeleteAllByCriteria(T bean);

    /// <summary>
    /// Supprime plusieurs beans à partir de leur clé primaire.
    /// </summary>
    /// <param name="primaryKeys">Clef primaires des objets.</param>
    void DeleteCollection(ICollection<int> primaryKeys);

    /// <summary>
    /// Retourne un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <returns>Bean.</returns>
    T? Get(object primaryKey);

    /// <summary>
    /// Retourne tous les beans pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    IList<T> GetAll(QueryParameter? queryParameter = null);

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="criteria">Liste des critères.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    IList<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter? queryParameter = null);

    /// <summary>
    /// Retourne tous les beans pour un type suivant
    /// une liste de critères donnés.
    /// </summary>
    /// <param name="bean">Bean de critère.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <returns>Collection.</returns>
    IList<T> GetAllByCriteria(T bean, QueryParameter? queryParameter = null);

    /// <summary>
    /// Retourne un bean à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    T? GetByCriteria(FilterCriteria criteria);

    /// <summary>
    /// Retourne un bean à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <returns>Bean.</returns>
    T? GetByCriteria(T criteria);

    /// <summary>
    /// Insère un élément.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    object Insert(T bean, ColumnSelector? columnSelector = null);

    /// <summary>
    /// Insére l'ensemble des éléments.
    /// </summary>
    /// <param name="values">Valeurs à insérer.</param>
    /// <returns>Valeurs insérées.</returns>
    ICollection<T> InsertAll(ICollection<T> values);

    /// <summary>
    /// Sauvegarde un bean (update si PK renseignée, insert sinon).
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    object Save(T bean, ColumnSelector? columnSelector = null);

    /// <summary>
    /// Sauvegarde l'ensemble des éléments d'une association n-n.
    /// </summary>
    /// <param name="values">Les valeurs à ajouter via associations.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
    void SaveAll(ICollection<T> values, ColumnSelector? columnSelector = null);
}
