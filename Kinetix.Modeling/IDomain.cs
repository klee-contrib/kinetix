using System.ComponentModel.DataAnnotations;
using Kinetix.Modeling.Exceptions;

namespace Kinetix.Modeling;

/// <summary>
/// Interface d'un domaine
/// Les domaines sont associés aux propriétés des objets métiers grâce à l'attribut DomainAttribute.
/// Un domaine porte :
///     - un type de données (type primitif)
///     - une contrainte responsable de vérifier que la donnée typée est dans les
///       plages de valeurs acceptées.
/// Pour un domaine portant sur un type string, la longueur maximum autorisée est
/// définie par une contrainte. Dans ce cas, la contrainte doit implémenter l'interface
/// IConstraintLength. Le domaine est ainsi en mesure de publier la longueur qui
/// lui est associée.
/// Un domaine ne définit pas si la donnée est obligatoire ou facultative.
/// </summary>
public interface IDomain
{
    /// <summary>
    /// Obtient le type de données du domaine.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Obtient la longueur maximale des données autorisées si elle est définie.
    /// </summary>
    int? Length { get; }

    /// <summary>
    /// Retourne les attributs de validation associés.
    /// </summary>
    ICollection<ValidationAttribute> ValidationAttributes { get; }

    /// <summary>
    /// Retourne les autres attributs.
    /// </summary>
    ICollection<Attribute> ExtraAttributes { get; }

    /// <summary>
    /// Vérifie la cohérence de la propriété
    /// avec le domaine.
    /// </summary>
    /// <param name="propertyDescriptor">Propriété.</param>
    void CheckPropertyType(BeanPropertyDescriptor propertyDescriptor);

    /// <summary>
    /// Teste si la valeur passée en paramètre est valide pour le champ.
    /// </summary>
    /// <param name="value">Valeur à tester.</param>
    /// <param name="propertyDescriptor">Propriété.</param>
    /// <exception cref="System.InvalidCastException">En cas d'erreur de type.</exception>
    /// <exception cref="Exceptions.BusinessException">En cas d'erreur, le message décrit l'erreur.</exception>
    ErrorMessageCollection CheckValue(object value, BeanPropertyDescriptor propertyDescriptor);
}
