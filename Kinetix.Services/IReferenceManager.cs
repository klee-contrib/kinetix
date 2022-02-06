namespace Kinetix.Services;

/// <summary>
/// Manager pour les listes de référence.
/// </summary>
public interface IReferenceManager
{
    /// <summary>
    /// La liste des listes de références du manager.
    /// </summary>
    IEnumerable<string> ReferenceLists { get; }

    /// <summary>
    /// Vérifie que les valeurs de propriétés de réference d'un bean sont valides.
    /// </summary>
    /// <param name="bean">Le bean.</param>
    void CheckReferenceKeys(object bean);

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence à vider.</param>
    void FlushCache(string referenceName = null);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <param name="type">Le type de la liste de référence.</param>
    /// <returns>La liste de référence.</returns>
    ICollection<object> GetReferenceList(Type type);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence</param>
    /// <returns>La liste de référence.</returns>
    ICollection<object> GetReferenceList(string referenceName);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="referenceName">Le nom de la liste de référence</param>
    /// <returns>La liste de référence.</returns>
    ICollection<T> GetReferenceList<T>(string referenceName = null);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="referenceName">Le nom de la liste de référence</param>
    ICollection<T> GetReferenceList<T>(Func<T, bool> predicate, string referenceName = null);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKeys">Un array de clés primaires.</param>
    ICollection<T> GetReferenceList<T>(object[] primaryKeys);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="criteria">Un objet de critère.</param>
    ICollection<T> GetReferenceList<T>(T criteria);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    T GetReferenceObject<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    T GetReferenceObject<T>(object primaryKey);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    string GetReferenceValue<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    string GetReferenceValue<T>(object primaryKey);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="reference">L'object dont on veut la valeur.</param>
    string GetReferenceValue<T>(T reference);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <param name="type">Le type de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    string GetReferenceValue(Type type, object primaryKey);
}
