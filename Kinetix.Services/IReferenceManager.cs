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
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task CheckReferenceKeysAsync(object? bean, CancellationToken ct = default);

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence à vider.</typeparam>
    /// <returns>Task.</returns>
    void FlushCache<T>();

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>Task.</returns>
    void FlushCache(string referenceName);

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <typeparam name="T">Le type de la liste de référence à vider.</typeparam>
    /// <returns>Task.</returns>
    Task FlushCacheAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// Vide le cache de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task FlushCacheAsync(string referenceName, CancellationToken ct = default);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>La liste de référence.</returns>
    ICollection<T> GetReferenceList<T>();

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Liste de référence.</returns>
    ICollection<T> GetReferenceList<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>La liste de référence.</returns>
    ICollection<object> GetReferenceList(string referenceName);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>La liste de référence.</returns>
    Task<ICollection<T>> GetReferenceListAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Liste de référence.</returns>
    Task<ICollection<T>> GetReferenceListAsync<T>(Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>
    /// Récupère une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>La liste de référence.</returns>
    Task<ICollection<object>> GetReferenceListAsync(string referenceName, CancellationToken ct = default);

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>La liste de référence.</returns>
    IDictionary<object, T> GetReferenceMap<T>();

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Liste de référence.</returns>
    IDictionary<object, T> GetReferenceMap<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>La liste de référence.</returns>
    Task<IDictionary<object, T>> GetReferenceMapAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// Récupère une liste de référence sous forme de map clé primaire => objet.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Liste de référence.</returns>
    Task<IDictionary<object, T>> GetReferenceMapAsync<T>(Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Objet.</returns>
    T? GetReferenceObject<T>(object? primaryKey);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Objet.</returns>
    T? GetReferenceObject<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Objet.</returns>
    object? GetReferenceObject(string referenceName, object? primaryKey);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Objet.</returns>
    Task<T?> GetReferenceObjectAsync<T>(object? primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Objet.</returns>
    Task<T?> GetReferenceObjectAsync<T>(Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>
    /// Récupère un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Objet.</returns>
    Task<object?> GetReferenceObjectAsync(string referenceName, object? primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue<T>(object? primaryKey);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <returns>Valeur.</returns>
    string? GetReferenceValue(string referenceName, object? primaryKey);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Valeur.</returns>
    Task<string?> GetReferenceValueAsync<T>(object? primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Valeur.</returns>
    Task<string?> GetReferenceValueAsync<T>(Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>
    /// Récupère la valeur d'un objet d'une liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="primaryKey">Une clé primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Valeur.</returns>
    Task<string?> GetReferenceValueAsync(string referenceName, object? primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap<T>();

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap<T>(Func<T, bool> predicate);

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>Map clé => valeur.</returns>
    IDictionary<object, string> GetReferenceValueMap(string referenceName);

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Map clé => valeur.</returns>
    Task<IDictionary<object, string>> GetReferenceValueMapAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <typeparam name="T">Le type de la liste de référence.</typeparam>
    /// <param name="predicate">Un prédicat pour filtrer la liste.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Map clé => valeur.</returns>
    Task<IDictionary<object, string>> GetReferenceValueMapAsync<T>(
        Func<T, bool> predicate,
        CancellationToken ct = default
    );

    /// <summary>
    /// Récupère une map clé primaire => valeur pour une liste de référence donnée.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Map clé => valeur.</returns>
    Task<IDictionary<object, string>> GetReferenceValueMapAsync(string referenceName, CancellationToken ct = default);
}
