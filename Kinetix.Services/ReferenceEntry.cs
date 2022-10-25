namespace Kinetix.Services;

/// <summary>
/// Entrée pour une liste de référence.
/// </summary>
internal class ReferenceEntry<T>
{
    /// <summary>
    /// Liste de référence.
    /// </summary>
    public IDictionary<string, T> Map { get; set; }

    /// <summary>
    /// Retourne un objet de référence.
    /// </summary>
    /// <param name="predicate">Prédicat.</param>
    /// <returns>Objet.</returns>
    public T GetReferenceObject(Func<T, bool> predicate)
    {
        return Map.SingleOrDefault(item => predicate(item.Value)).Value;
    }

    /// <summary>
    /// Retourne un objet de référence.
    /// </summary>
    /// <param name="primaryKey">Clé primaire.</param>
    /// <returns>Objet.</returns>
    public T GetReferenceObject(object primaryKey)
    {
        Map.TryGetValue(primaryKey.ToString(), out var value);
        return value;
    }
}
