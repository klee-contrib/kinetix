using System.ComponentModel.DataAnnotations;
using Kinetix.Modeling.Annotations;
using Kinetix.Modeling.Exceptions;

namespace Kinetix.Modeling;

/// <summary>
/// Cette classe décrit un domaine.
/// Les domaines sont associés aux propriétés des objets métiers grâce à
/// la propriété DomainName de l'attribut DataDescriptionAttribute.
/// Un domaine porte :
///     - un type de données (type primitif)
///     - une contrainte responsable de vérifier que la donnée typée est dans les
///       plages de valeurs acceptées.
/// Pour un domaine portant sur un type string, la longueur maximum autorisé est
/// définie par une contrainte. Dans ce cas, la contrainte doit implémenter l'interface
/// IConstraintLength. Le domaine est ainsi en mesure de publier la longueur qui
/// lui est associé.
/// Un domaine ne définit pas si la données est obligatoire ou facultative.
/// </summary>
/// <remarks>
/// Crée un nouveau domaine.
/// Le formateur et la contrainte sont facultatifs.
/// </remarks>
/// <param name="name">Nom.</param>
/// <param name="validationAttributes">Attributs gérant la validation de la donnée.</param>
/// <param name="extraAttributes">Autres attributs.</param>
public sealed class Domain(
    Enum name,
    ICollection<ValidationAttribute> validationAttributes,
    ICollection<Attribute> extraAttributes
)
{
    /// <summary>
    /// Liste des attributs de validation.
    /// </summary>
    public ICollection<ValidationAttribute> ValidationAttributes { get; private set; } = validationAttributes;

    /// <summary>
    /// Liste des autres attributs.
    /// </summary>
    public ICollection<Attribute> ExtraAttributes { get; private set; } = extraAttributes;

    /// <summary>
    /// Nom du domaine.
    /// </summary>
    public Enum Name { get; } = name;

    /// <summary>
    /// Obtient la longueur maximale des données autorisées si elle est définie.
    /// </summary>
    public int? Length
    {
        get
        {
            var strLenAttr = GetValidationAttribute<StringLengthAttribute>();
            if (strLenAttr != null)
            {
                return strLenAttr.MaximumLength;
            }

            var ranAttr = GetValidationAttribute<RangeAttribute>();
            if (ranAttr != null && ranAttr.Maximum != null)
            {
                return ranAttr.Maximum.ToString()!.Length;
            }

            var siretAttr = GetValidationAttribute<NumeroSiretAttribute>();
            return siretAttr != null ? NumeroSiretAttribute.SiretLength : (int?)null;
        }
    }

    /// <summary>
    /// Teste si la valeur passée en paramètre est valide pour le champ.
    /// </summary>
    /// <param name="value">Valeur à tester.</param>
    /// <param name="propertyDescriptor">Propriété.</param>
    /// <exception cref="InvalidCastException">En cas d'erreur de type.</exception>
    /// <exception cref="BusinessException">En cas d'erreur, le message décrit l'erreur.</exception>
    /// <returns>Messages d'erreur.</returns>
    public ErrorMessageCollection CheckValue(object? value, BeanPropertyDescriptor propertyDescriptor)
    {
        return propertyDescriptor == null ? throw new ArgumentNullException(nameof(propertyDescriptor))
            : ValidationAttributes != null
                ? new ErrorMessageCollection(
                    ValidationAttributes
                        .Where(va => !va.IsValid(value))
                        .Select(va => va.FormatErrorMessage(propertyDescriptor.PropertyName))
                )
            : new ErrorMessageCollection();
    }

    /// <summary>
    /// Obtient la valeur d'un attribut de validation à partir de son type s'il a été défini, null sinon.
    /// </summary>
    /// <param name="attributeType">Type de l'attribut de validation.</param>
    /// <returns>Valeur de l'attribut.</returns>
    public Attribute? GetValidationAttribute(Type attributeType)
    {
        ArgumentNullException.ThrowIfNull(attributeType);

        if (ValidationAttributes == null)
        {
            return null;
        }

        foreach (Attribute attr in ValidationAttributes)
        {
            if (attributeType.IsInstanceOfType(attr))
            {
                return attr;
            }
        }

        return null;
    }

    /// <summary>
    /// Obtient la valeur d'un attribut de validation à partir de son type s'il a été défini, null sinon.
    /// </summary>
    /// <returns>Valeur de l'attribut.</returns>
    /// <typeparam name="TValidation">Type de l'attribut de validation.</typeparam>
    public TValidation? GetValidationAttribute<TValidation>()
        where TValidation : class
    {
        if (ValidationAttributes == null)
        {
            return default;
        }

        foreach (var attr in ValidationAttributes)
        {
            if (attr is TValidation valAttr)
            {
                return valAttr;
            }
        }

        return default;
    }
}
