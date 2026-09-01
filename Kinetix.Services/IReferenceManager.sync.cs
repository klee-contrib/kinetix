namespace Kinetix.Services;

/// <summary>
/// Manager pour les listes de référence.
/// </summary>
public partial interface IReferenceManager
{
    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence à vider.</typeparam>
    /// <returns>Task.</returns>
    void FlushCache<T>()
        where T : notnull;

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>Task.</returns>
    void FlushCache(string referenceName);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>La liste de référence.</returns>
    ICollection<T> GetReferenceList<T>()
        where T : notnull;

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Liste de référence.</returns>
    ICollection<T> GetReferenceList<T>(Func<T, bool> predicate)
        where T : notnull;

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>La liste de référence.</returns>
    ICollection<object> GetReferenceList(string referenceName);

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>La liste de référence.</returns>
    IDictionary<object, T> GetReferenceMap<T>()
        where T : notnull;

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Liste de référence.</returns>
    IDictionary<object, T> GetReferenceMap<T>(Func<T, bool> predicate)
        where T : notnull;

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Objet.</returns>
    T? GetReferenceObject<T>(object? primaryKey)
        where T : notnull;

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Objet.</returns>
    T? GetReferenceObject<T>(Func<T, bool> predicate)
        where T : notnull;

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Objet.</returns>
    object? GetReferenceObject(string referenceName, object? primaryKey);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue<T>(object? primaryKey)
        where T : notnull;

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue<T>(Func<T, bool> predicate)
        where T : notnull;

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue(string referenceName, object? primaryKey);

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap<T>()
        where T : notnull;

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap<T>(Func<T, bool> predicate)
        where T : notnull;

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap(string referenceName);
}
