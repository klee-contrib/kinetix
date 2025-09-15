using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Kinetix.Modeling.Annotations;

namespace Kinetix.Modeling;

/// <summary>
/// Classe pour la gestion des domaines.
/// </summary>
internal static class DomainManager
{
    /// <summary>
    /// Dictionnaire des domaines.
    /// </summary>
    private static readonly ConcurrentDictionary<Enum, Domain> _domainDictionary = new();

    /// <summary>
    /// Récupère le domaine d'une propriété.
    /// </summary>
    /// <param name="property">Propriété.</param>
    /// <returns>Domaine.</returns>
    internal static Domain GetDomain(BeanPropertyDescriptor property)
    {
        Domain domain = null;
        ArgumentNullException.ThrowIfNull(property);

        if (property.DomainName == null)
        {
            var primitiveType = property.PrimitiveType;
            if (primitiveType != null)
            {
                var builtInDomain = typeof(BuiltInDomains)
                    .GetMembers()
                    .SingleOrDefault(p => p.GetCustomAttribute<DomainTypeAttribute>()?.Type == primitiveType);
                domain =
                    builtInDomain != null
                        ? GetDomain(
                            Enum.GetValues(typeof(BuiltInDomains))
                                .Cast<Enum>()
                                .Single(e => e.ToString() == builtInDomain.Name)
                        )
                        : throw new NotSupportedException(
                            "Pas de domaine par défaut pour le type " + primitiveType.Name + " !"
                        );
            }
        }
        else
        {
            domain = GetDomain(property.DomainName);
        }

        return domain;
    }

    private static Domain GetDomain(Enum domainName)
    {
        return _domainDictionary.GetOrAdd(
            domainName,
            d =>
            {
                var property = d.GetType().GetMember(d.ToString())[0];
                var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
                var extraAttributes = new List<Attribute>();

                foreach (var attribute in property.GetCustomAttributes(false))
                {
                    if (
                        attribute is DomainAttribute
                        || attribute is TypeConverterAttribute
                        || attribute is ValidationAttribute
                    )
                    {
                        continue;
                    }

                    extraAttributes.Add(attribute as Attribute);
                }

                return new Domain(d, validationAttributes.ToList(), extraAttributes);
            }
        );
    }
}
