﻿namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Attribut définissant le domaine d'une propriété.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DomainAttribute : Attribute
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="name">Nom du domaine.</param>
    public DomainAttribute(object name)
    {
        Name = (Enum)name;
    }

    /// <summary>
    /// Obtient le nom du domaine.
    /// </summary>
    public Enum Name
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient ou définit le type contenant les messages d'erreurs.
    /// </summary>
    public Type ErrorMessageResourceType
    {
        get;
        set;
    }

    /// <summary>
    /// Obtient ou définit le nom de la clef de ressource.
    /// </summary>
    public string ErrorMessageResourceName
    {
        get;
        set;
    }

    /// <summary>
    /// Obtient ou définit le suffix de la propriété portant les métadonnées utilent au domaine.
    /// </summary>
    public string MetadataPropertySuffix
    {
        get;
        set;
    }
}
