using System.ComponentModel;

namespace Kinetix.Modeling;

/// <summary>
/// Classe de description d'une propriété.
/// </summary>
public sealed class BeanPropertyDescriptor
{
    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété.</param>
    /// <param name="memberName">Nom du membre (DataMemberAttribute).</param>
    /// <param name="propertyType">Type de la propriété.</param>
    /// <param name="description">Description de la propriété.</param>
    /// <param name="isPrimaryKey">True si la propriété correspond à la clef primaire.</param>
    /// <param name="isDefault">True si la propriété est la propriété par défaut de l'objet.</param>
    /// <param name="isRequired">True si la propriété doit être renseignée.</param>
    /// <param name="referenceType">Type de la liste de référence associée à la propriété.</param>
    /// <param name="isReadOnly"><code>True</code> si la propriété est en lecture seule, <code>False</code> sinon.</param>
    /// <param name="isBrowsable">Indique si la propriété est affichable.</param>
    /// <param name="extraAttributes">Attributes supplémentaires.</param>
    internal BeanPropertyDescriptor(
        string propertyName,
        string? memberName,
        Type propertyType,
        string? description,
        bool isPrimaryKey,
        bool isDefault,
        bool isRequired,
        Type? referenceType,
        bool isReadOnly,
        bool isBrowsable,
        IList<Attribute> extraAttributes
    )
    {
        PropertyName = propertyName;
        MemberName = memberName;
        PropertyType = propertyType;
        Description = description;
        IsPrimaryKey = isPrimaryKey;
        IsDefault = isDefault;
        IsRequired = isRequired;
        ReferenceType = referenceType;
        IsReadOnly = isReadOnly;
        IsBrowsable = isBrowsable;
        ExtraAttributes = extraAttributes;

        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PrimitiveType = PropertyType.GetGenericArguments()[0];
        }
        else if (
            PropertyType.IsValueType
            || PropertyType == typeof(string)
            || PropertyType.GetInterface("ICollection") != null
        )
        {
            PrimitiveType = PropertyType;
        }
    }

    /// <summary>
    /// Obtient la description de la valeur.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Indique si la propriété est affichable à l'écran.
    /// </summary>
    public bool IsBrowsable { get; }

    /// <summary>
    /// Indique si la propriété est la propriété par défaut de l'objet.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    /// Indique si la propriété correspond à la clef primaire.
    /// </summary>
    public bool IsPrimaryKey { get; }

    /// <summary>
    /// Indique si la valeur est obligatoire.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Indique si la colonne est traduisible (dans le cas d'une liste de référence).
    /// </summary>
    public bool IsTranslatable { get; }

    /// <summary>
    /// Retourne le nom du membre porté par DataMemberAttribute.
    /// Pour les propriétés persistantes, il s'agit
    /// de la colonne base de données. Pour des objets issus
    /// d'un service Web, il s'agit du nom Soap de la propriété.
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Retourne le type primitif de la propriété.
    /// Si PropertyType vaut Nullable&lt;int&gt;, PrimitiveType vaut int.
    /// </summary>
    public Type? PrimitiveType { get; }

    /// <summary>
    /// Obtient le nom de la propriété.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Obtient le type de la propriété.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Obtient le type de l'objet de référence
    /// associé à la propriété.
    /// </summary>
    public Type? ReferenceType { get; }

    /// <summary>
    /// Retourne si la propriété est en lecture seule.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Attributs supplémentaires sur la propriété (hors attributs par défaut).
    /// </summary>
    public IList<Attribute> ExtraAttributes { get; }

    /// <summary>
    /// Retourne la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <returns>Valeur.</returns>
    public object? GetValue(object bean)
    {
        return TypeDescriptor.GetProperties(bean)[PropertyName]?.GetValue(bean);
    }

    /// <summary>
    /// Définit la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <param name="value">Valeur.</param>
    /// <exception cref="System.NotSupportedException">Si la propriété est en lecture seule.</exception>
    public void SetValue(object bean, object? value)
    {
        TypeDescriptor.GetProperties(bean)[PropertyName]?.SetValue(bean, value);
    }

    /// <summary>
    /// Retourne une chaîne de caractère représentant l'objet.
    /// </summary>
    /// <returns>Chaîne de caractère représentant l'objet.</returns>
    public override string ToString()
    {
        return PropertyName;
    }
}
