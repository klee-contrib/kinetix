namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Attribut définissant le domaine d'une propriété.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="name">Nom du domaine.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DomainAttribute(object name) : Attribute
{
    /// <summary>
    /// Obtient le nom du domaine.
    /// </summary>
    public Enum Name { get; private set; } = (Enum)name;
}
