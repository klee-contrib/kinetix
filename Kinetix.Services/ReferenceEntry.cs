namespace Kinetix.Services;

/// <summary>
/// Entrée pour une liste de référence.
/// </summary>
internal class ReferenceEntry<T>
{
    private readonly string _name;

    /// <summary>
    /// Crée une nouvelle entrée pour le type.
    /// </summary>
    /// <param name="name">Nom du bean.</param>
    public ReferenceEntry(string name)
    {
        _name = name;
    }

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
        return Map.Single(item => predicate(item.Value)).Value;
    }

    /// <summary>
    /// Retourne un objet de référence.
    /// </summary>
    /// <param name="primaryKey">Clé primaire.</param>
    /// <returns>Objet.</returns>
    public T GetReferenceObject(object primaryKey)
    {
        /* Cherche la valeur pour la locale demandée. */
        if (Map.TryGetValue(primaryKey.ToString(), out var value))
        {
            return value;
        }
        else
        {
            throw new NotSupportedException($"Reference entry {primaryKey} is missing for {_name}.");
        }
    }
}
