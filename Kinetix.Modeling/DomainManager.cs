using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Kinetix.Modeling.Annotations;

namespace Kinetix.Modeling;

/// <summary>
/// Classe pour la gestion des domaines.
/// </summary>
public static class DomainManager
{
    /// <summary>
    /// Dictionnaire des domaines.
    /// </summary>
    private readonly static ConcurrentDictionary<Enum, IDomain> _domainDictionary = new();

    /// <summary>
    /// Récupère le domaine d'une propriété.
    /// </summary>
    /// <param name="property">Propriété.</param>
    /// <returns>Domaine.</returns>
    public static IDomain GetDomain(BeanPropertyDescriptor property)
    {
        IDomain domain = null;
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        if (property.DomainName == null)
        {
            var primitiveType = property.PrimitiveType;
            if (primitiveType != null)
            {
                var builtInDomain = typeof(BuiltInDomains).GetMembers().SingleOrDefault(p => p.GetCustomAttribute<DomainTypeAttribute>()?.Type == primitiveType);
                domain = builtInDomain != null
                    ? GetDomain(Enum.GetValues(typeof(BuiltInDomains)).Cast<Enum>().Single(e => e.ToString() == builtInDomain.Name))
                    : throw new NotSupportedException("Pas de domaine par défaut pour le type " + primitiveType.Name + " !");
            }
        }
        else
        {
            domain = GetDomain(property.DomainName);
        }

        if (domain != null)
        {
            domain.CheckPropertyType(property);
        }

        return domain;
    }

    private static IDomain GetDomain(Enum domainName)
    {
        return _domainDictionary.GetOrAdd(domainName, _ =>
        {
            var property = domainName.GetType().GetMember(domainName.ToString())[0];
            var domainType = property.GetCustomAttribute<DomainTypeAttribute>().Type;

            var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();

            var extraAttributes = new List<Attribute>();
            foreach (var attribute in property.GetCustomAttributes(false))
            {
                if (attribute is DomainAttribute || attribute is TypeConverterAttribute || attribute is ValidationAttribute)
                {
                    continue;
                }

                extraAttributes.Add(attribute as Attribute);
            }

            return (IDomain)Activator.CreateInstance(
                typeof(Domain<>).MakeGenericType(domainType),
                domainName,
                validationAttributes,
                extraAttributes);
        });
    }
}
