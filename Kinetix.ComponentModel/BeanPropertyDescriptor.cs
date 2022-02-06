using System.ComponentModel;
using System.Globalization;
using Kinetix.ComponentModel.Exceptions;

namespace Kinetix.ComponentModel;

/// <summary>
/// Classe de description d'une propriété.
/// </summary>
public sealed class BeanPropertyDescriptor
{
    private IDomain _domainChecker;

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="domainManager">DomainManager.</param>
    /// <param name="propertyName">Nom de la propriété.</param>
    /// <param name="memberName">Nom du membre (DataMemberAttribute).</param>
    /// <param name="propertyType">Type de la propriété.</param>
    /// <param name="description">Description de la propriété.</param>
    /// <param name="domainName">Domaine de la propriété.</param>
    /// <param name="isPrimaryKey">True si la propriété correspond à la clef primaire.</param>
    /// <param name="isDefault">True si la propriété est la propriété par défaut de l'objet.</param>
    /// <param name="isRequired">True si la propriété doit être renseignée.</param>
    /// <param name="referenceType">Type de la liste de référence associée à la propriété.</param>
    /// <param name="isReadOnly"><code>True</code> si la propriété est en lecture seule, <code>False</code> sinon.</param>
    /// <param name="isBrowsable">Indique si la propriété est affichable.</param>
    internal BeanPropertyDescriptor(
        string propertyName,
        string memberName,
        Type propertyType,
        string description,
        Enum domainName,
        bool isPrimaryKey,
        bool isDefault,
        bool isRequired,
        Type referenceType,
        bool isReadOnly,
        bool isBrowsable)
    {
        PropertyName = propertyName;
        MemberName = memberName;
        PropertyType = propertyType;
        Description = description;
        DomainName = domainName;
        IsPrimaryKey = isPrimaryKey;
        IsDefault = isDefault;
        IsRequired = isRequired;
        ReferenceType = referenceType;
        IsReadOnly = isReadOnly;
        IsBrowsable = isBrowsable;

        InitPrimitiveType();
        CheckPropertyTypeForReference();
    }

    /// <summary>
    /// Obtient la description de la valeur.
    /// </summary>
    public string Description
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient le nom du domaine.
    /// </summary>
    public Enum DomainName
    {
        get;
        private set;
    }

    /// <summary>
    /// Indique si la propriété est affichable à l'écran.
    /// </summary>
    public bool IsBrowsable
    {
        get;
        private set;
    }

    /// <summary>
    /// Indique si la propriété est la propriété par défaut de l'objet.
    /// </summary>
    public bool IsDefault
    {
        get;
        private set;
    }

    /// <summary>
    /// Indique si la propriété correspond à la clef primaire.
    /// </summary>
    public bool IsPrimaryKey
    {
        get;
        private set;
    }

    /// <summary>
    /// Indique si la valeur est obligatoire.
    /// </summary>
    public bool IsRequired
    {
        get;
        private set;
    }

    /// <summary>
    /// Indique si la colonne est traduisible (dans le cas d'une liste de référence).
    /// </summary>
    public bool IsTranslatable
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne le nom du membre porté par DataMemberAttribute.
    /// Pour les propriétés persistantes, il s'agit
    /// de la colonne base de données. Pour des objets issus
    /// d'un service Web, il s'agit du nom Soap de la propriété.
    /// </summary>
    public string MemberName
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne le type primitif de la propriété.
    /// Si PropertyType vaut Nullable&lt;int&gt;, PrimitiveType vaut int.
    /// </summary>
    public Type PrimitiveType
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient le nom de la propriété.
    /// </summary>
    public string PropertyName
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient le type de la propriété.
    /// </summary>
    public Type PropertyType
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient le type de l'objet de référence
    /// associé à la propriété.
    /// </summary>
    public Type ReferenceType
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne si la propriété est en lecture seule.
    /// </summary>
    public bool IsReadOnly
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne le domaine associé à la propriété.
    /// </summary>
    public IDomain Domain
    {
        get
        {
            if (_domainChecker == null)
            {
                _domainChecker = DomainManager.GetDomain(this);
            }

            return _domainChecker;
        }
    }

    /// <summary>
    /// Vérifie que le type d'une valeur correspond bien au type
    /// de la propriété.
    /// </summary>
    /// <param name="value">Valeur à tester.</param>
    public void CheckValueType(object value)
    {
        if (value == null)
        {
            return;
        }

        var valueType = value.GetType();
        if (valueType.IsPrimitive || typeof(decimal).Equals(valueType) || typeof(Guid).Equals(valueType) ||
                typeof(string).Equals(valueType) || typeof(DateTime).Equals(valueType)
                || typeof(byte[]).Equals(valueType) || typeof(TimeSpan).Equals(valueType))
        {
            if (!valueType.Equals(PrimitiveType))
            {
                throw new InvalidCastException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionInvalidValueType,
                        valueType,
                        PropertyType,
                        PropertyName));
            }

            return;
        }

        throw new NotSupportedException(string.Format(
            CultureInfo.CurrentUICulture,
            SR.ExceptionTypeDescription,
            PropertyType));
    }

    /// <summary>
    /// Retourne la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <returns>Valeur.</returns>
    public object GetValue(object bean)
    {
        return TypeDescriptor.GetProperties(bean)[PropertyName].GetValue(bean);
    }

    /// <summary>
    /// Définit la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <param name="value">Valeur.</param>
    /// <exception cref="System.NotSupportedException">Si la propriété est en lecture seule.</exception>
    public void SetValue(object bean, object value)
    {
        var descriptor = TypeDescriptor.GetProperties(bean)[PropertyName];
        if (descriptor.IsReadOnly)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SR.ReadOnlyProperty, PropertyName));
        }

        descriptor.SetValue(bean, value);
        return;
    }

    /// <summary>
    /// Retourne une chaîne de caractère représentant l'objet.
    /// </summary>
    /// <returns>Chaîne de caractère représentant l'objet.</returns>
    public override string ToString()
    {
        return PropertyName;
    }

    /// <summary>
    /// Valide les contraintes de domaine pour une propriété.
    /// </summary>
    /// <param name="value">Valeur de la propriété.</param>
    internal ErrorMessageCollection CheckDomain(object value)
    {
        return Domain.CheckValue(value, this);
    }

    /// <summary>
    /// Vérifie la cohérence du type de la propriété si elle est associée
    /// à une liste de référence.
    /// </summary>
    private void CheckPropertyTypeForReference()
    {
        if (MemberName != null && ReferenceType != null && PrimitiveType != typeof(int) && PrimitiveType != typeof(string) && PrimitiveType != typeof(Guid))
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SR.ExceptionTypeInt32Required, PropertyType, PropertyName));
        }
    }

    /// <summary>
    /// Initialise le type primitif de la propriété.
    /// </summary>
    private void InitPrimitiveType()
    {
        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PrimitiveType = PropertyType.GetGenericArguments()[0];
            return;
        }

        if (PropertyType.IsValueType || PropertyType == typeof(string) || PropertyType.GetInterface("ICollection") != null)
        {
            PrimitiveType = PropertyType;
            return;
        }

        PrimitiveType = null;
    }
}
