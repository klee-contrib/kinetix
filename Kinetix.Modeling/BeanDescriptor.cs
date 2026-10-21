using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Kinetix.Modeling.Annotations;
using Kinetix.Modeling.Exceptions;

namespace Kinetix.Modeling;

/// <summary>
/// Fournit la description d'un bean.
/// </summary>
public static class BeanDescriptor
{
    /// <summary>
    /// Nom par défaut de la propriété par défaut d'un bean, pour l'affichage du libellé de l'objet.
    /// </summary>
    private const string DefaultPropertyDefaultName = "Libelle";

    private static readonly ConcurrentDictionary<Type, BeanDefinition> _beanDefinitionDictionnary = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _resourceTypeMap = new();

    /// <summary>
    /// Vérifie les contraintes sur un bean.
    /// </summary>
    /// <param name="bean">Bean à vérifier.</param>
    /// <param name="propertiesToCheck">Si renseigné, seules ces propriétés seront validées.</param>
    public static void Check(object bean, IEnumerable<string>? propertiesToCheck = null)
    {
        if (bean != null)
        {
            GetDefinition(bean.GetType())?.Check(bean, propertiesToCheck);
        }
    }

    /// <summary>
    /// Retourne la definition d'un bean.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Description des propriétés.</returns>
    public static BeanDefinition GetDefinition(Type beanType)
    {
        var descriptionType = beanType;

        if (!_beanDefinitionDictionnary.TryGetValue(descriptionType, out var definition))
        {
            var properties = CreateBeanPropertyCollection(beanType);
            var table = beanType.GetCustomAttribute<TableAttribute>();
            var contractName = table?.Name;

            var reference = beanType.GetCustomAttribute<ReferenceAttribute>();
            var isReference = reference != null;

            definition = new BeanDefinition(beanType, properties, contractName, isReference);

            _beanDefinitionDictionnary.TryAdd(descriptionType, definition);
        }

        return definition;
    }

    /// <summary>
    /// Vérifie les contraintes sur un bean.
    /// </summary>
    /// <param name="bean">Bean à vérifier.</param>
    /// <param name="propertiesToCheck">Si renseigné, seules ces propriétés seront validées.</param>
    /// <returns>Les erreurs.</returns>
    public static ErrorMessageCollection GetErrors(object bean, IEnumerable<string>? propertiesToCheck = null)
    {
        if (bean != null)
        {
            return GetDefinition(bean.GetType())?.GetErrors(bean, propertiesToCheck) ?? [];
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Retourne la description des propriétés d'un objet sous forme d'une collection.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Description des propriétés.</returns>
    private static BeanPropertyDescriptorCollection CreateBeanPropertyCollection(Type beanType)
    {
        var properties = TypeDescriptor.GetProperties(beanType);
        var defaultProperty = TypeDescriptor.GetDefaultProperty(beanType);
        return CreateCollection(properties, defaultProperty);
    }

    /// <summary>
    /// Crée la collection des descripteurs de propriétés.
    /// </summary>
    /// <param name="properties">PropertyDescriptors.</param>
    /// <param name="defaultProperty">Propriété par défaut.</param>
    /// <returns>Collection.</returns>
    private static BeanPropertyDescriptorCollection CreateCollection(
        PropertyDescriptorCollection properties,
        PropertyDescriptor? defaultProperty
    )
    {
        var coll = new BeanPropertyDescriptorCollection();
        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];

            var keyAttr = (KeyAttribute?)property.Attributes[typeof(KeyAttribute)];
            var displayAttr = (DisplayAttribute?)property.Attributes[typeof(DisplayAttribute)];
            var attr = (ReferencedTypeAttribute?)property.Attributes[typeof(ReferencedTypeAttribute)];
            var colAttr = (ColumnAttribute?)property.Attributes[typeof(ColumnAttribute)];
            var requiredAttr = (RequiredAttribute?)property.Attributes[typeof(RequiredAttribute)];

            var extraAttributes = property
                .Attributes.Cast<Attribute>()
                .Where(a =>
                    a
                        is not KeyAttribute
                            and not DisplayAttribute
                            and not ReferencedTypeAttribute
                            and not ColumnAttribute
                            and not RequiredAttribute
                )
                .ToList();

            string? display = null;
            if (displayAttr != null)
            {
                if (displayAttr.ResourceType != null && displayAttr.Name != null)
                {
                    if (!_resourceTypeMap.TryGetValue(displayAttr.ResourceType, out var resourceProperties))
                    {
                        resourceProperties = [];
                        _resourceTypeMap.TryAdd(displayAttr.ResourceType, resourceProperties);

                        foreach (
                            var p in displayAttr.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                        )
                        {
                            resourceProperties.Add(p.Name, p);
                        }
                    }

                    display = resourceProperties[displayAttr.Name].GetValue(null, index: null)?.ToString();
                }
                else
                {
                    display = displayAttr.Name;
                }
            }

            var memberName = colAttr?.Name;
            var isPrimaryKey = keyAttr != null;
            var isRequired = requiredAttr != null;
            var isDefault =
                property.Equals(defaultProperty)
                || DefaultPropertyDefaultName.Equals(property.Name) && defaultProperty == null;
            var referenceType = attr?.ReferenceType;
            var isBrowsable = property.IsBrowsable;
            var isReadonly = property.IsReadOnly;

            var description = new BeanPropertyDescriptor(
                property.Name,
                memberName,
                property.PropertyType,
                display,
                isPrimaryKey,
                isDefault,
                isRequired,
                referenceType,
                isReadonly,
                isBrowsable,
                extraAttributes
            );

            coll.Add(description);
        }

        return coll;
    }
}
