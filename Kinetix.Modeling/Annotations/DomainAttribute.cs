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

    /// <summary>
    /// Obtient ou définit le type contenant les messages d'erreurs.
    /// </summary>
    public Type ErrorMessageResourceType { get; set; }

    /// <summary>
    /// Obtient ou définit le nom de la clef de ressource.
    /// </summary>
    public string ErrorMessageResourceName { get; set; }

    /// <summary>
    /// Obtient ou définit le suffix de la propriété portant les métadonnées utilent au domaine.
    /// </summary>
    public string MetadataPropertySuffix { get; set; }
}
