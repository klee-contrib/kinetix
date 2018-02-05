using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kinetix.ComponentModel.Annotations;
using Kinetix.ComponentModel.Exceptions;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Fournit la description d'un bean.
    /// </summary>
    public sealed class BeanDescriptor
    {
        /// <summary>
        /// Nom par défaut de la propriété par défaut d'un bean, pour l'affichage du libellé de l'objet.
        /// </summary>
        private const string DefaultPropertyDefaultName = "Libelle";

        private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _resourceTypeMap = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private readonly Dictionary<Type, BeanDefinition> _beanDefinitionDictionnary = new Dictionary<Type, BeanDefinition>();

        private readonly IDomainManager _domainManager;

        public BeanDescriptor(IDomainManager domainManager)
        {
            _domainManager = domainManager;
        }

        /// <summary>
        /// Vérifie les contraintes sur un bean.
        /// </summary>
        /// <param name="bean">Bean à vérifier.</param>
        /// <param name="allowPrimaryKeyNull">True si la clef primaire peut être null (insertion).</param>
        public void Check(object bean, bool allowPrimaryKeyNull = true)
        {
            if (bean != null)
            {
                try
                {
                    GetDefinition(bean).Check(bean, allowPrimaryKeyNull);
                }
                catch (ConstraintException e)
                {
                    throw new ConstraintException(e.Property, e.Property.Description + " " + e.Message, e);
                }
            }
        }

        /// <summary>
        /// Vérifie les contraintes sur les éléments contenus dans une collection.
        /// </summary>
        /// <param name="collection">Collection à vérifier.</param>
        /// <param name="allowPrimaryKeyNull">True si la clef primaire peut être null (insertion).</param>
        /// <typeparam name="T">Type des éléments de la collection.</typeparam>
        public void CheckAll<T>(ICollection<T> collection, bool allowPrimaryKeyNull = true)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            foreach (T obj in collection)
            {
                Check(obj, allowPrimaryKeyNull);
            }
        }

        /// <summary>
        /// Retourne la definition des beans d'une collection générique.
        /// </summary>
        /// <param name="collection">Collection générique de bean.</param>
        /// <returns>Description des propriétés des beans.</returns>
        public BeanDefinition GetCollectionDefinition(object collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            Type collectionType = collection.GetType();
            if (collectionType.IsArray)
            {
                return GetDefinition(collectionType.GetElementType());
            }

            if (!collectionType.IsGenericType)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionTypeDescription,
                        collection.GetType().FullName),
                    "collection");
            }

            Type genericDefinition = collectionType.GetGenericTypeDefinition();
            if (genericDefinition.GetInterface(typeof(ICollection<>).Name) == null)
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionNotSupportedGeneric,
                        genericDefinition.Name));
            }

            Type objectType = collectionType.GetGenericArguments()[0];
            ICollection coll = (ICollection)collection;
            if (typeof(ICustomTypeDescriptor).IsAssignableFrom(objectType) && coll.Count != 0)
            {
                object customObject = coll.Cast<object>().FirstOrDefault();
                return GetDefinition(customObject);
            }

            foreach (object obj in coll)
            {
                objectType = obj.GetType();
                break;
            }

            return GetDefinition(objectType, true);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <returns>Description des propriétés.</returns>
        public BeanDefinition GetDefinition(object bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException("bean");
            }

            return GetDefinitionInternal(bean.GetType(), bean);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="ignoreCustomTypeDesc">Si true, retourne un définition même si le type implémente ICustomTypeDescriptor.</param>
        /// <returns>Description des propriétés.</returns>
        public BeanDefinition GetDefinition(Type beanType, bool ignoreCustomTypeDesc = false)
        {
            if (beanType == null)
            {
                throw new ArgumentNullException("beanType");
            }

            if (!ignoreCustomTypeDesc && typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType))
            {
                throw new NotSupportedException(SR.ExceptionICustomTypeDescriptorNotSupported);
            }

            return GetDefinitionInternal(beanType, null);
        }

        /// <summary>
        /// Efface la définition d'un bean du singleton.
        /// </summary>
        /// <param name="descriptionType">Type portant la description.</param>
        public void ClearDefinition(Type descriptionType)
        {
            ClearDefinitionCore(descriptionType);
        }

        /// <summary>
        /// Crée la collection des descripteurs de propriétés.
        /// </summary>
        /// <param name="properties">PropertyDescriptors.</param>
        /// <param name="defaultProperty">Propriété par défaut.</param>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Collection.</returns>
        private BeanPropertyDescriptorCollection CreateCollection(PropertyDescriptorCollection properties, PropertyDescriptor defaultProperty, Type beanType)
        {
            BeanPropertyDescriptorCollection coll = new BeanPropertyDescriptorCollection(beanType);
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];

                KeyAttribute keyAttr = (KeyAttribute)property.Attributes[typeof(KeyAttribute)];
                DisplayAttribute displayAttr = (DisplayAttribute)property.Attributes[typeof(DisplayAttribute)];
                ReferencedTypeAttribute attr = (ReferencedTypeAttribute)property.Attributes[typeof(ReferencedTypeAttribute)];
                ColumnAttribute colAttr = (ColumnAttribute)property.Attributes[typeof(ColumnAttribute)];
                DomainAttribute domainAttr = (DomainAttribute)property.Attributes[typeof(DomainAttribute)];
                RequiredAttribute requiredAttr = (RequiredAttribute)property.Attributes[typeof(RequiredAttribute)];
                Type[] genericArgumentArray = beanType.GetGenericArguments();

                string display = null;
                if (displayAttr != null)
                {
                    if (displayAttr.ResourceType != null && displayAttr.Name != null)
                    {
                        if (!_resourceTypeMap.TryGetValue(displayAttr.ResourceType, out Dictionary<string, PropertyInfo> resourceProperties))
                        {
                            resourceProperties = new Dictionary<string, PropertyInfo>();
                            _resourceTypeMap[displayAttr.ResourceType] = resourceProperties;

                            foreach (PropertyInfo p in displayAttr.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Static))
                            {
                                resourceProperties.Add(p.Name, p);
                            }
                        }

                        display = resourceProperties[displayAttr.Name].GetValue(null, null).ToString();
                    }
                    else
                    {
                        display = displayAttr.Name;
                    }
                }

                string memberName = colAttr?.Name;
                bool isPrimaryKey = keyAttr != null;
                bool isRequired = requiredAttr != null;
                string domainName = domainAttr?.Name;
                bool isDefault = property.Equals(defaultProperty) || (DefaultPropertyDefaultName.Equals(property.Name) && defaultProperty == null);
                Type referenceType = attr?.ReferenceType;
                bool isBrowsable = property.IsBrowsable;
                bool isReadonly = property.IsReadOnly;

                var description = new BeanPropertyDescriptor(
                    _domainManager,
                    property.Name,
                    memberName,
                    property.PropertyType,
                    display,
                    domainName,
                    isPrimaryKey,
                    isDefault,
                    isRequired,
                    referenceType,
                    isReadonly,
                    isBrowsable);
                if (domainName != null)
                {
                    _domainManager.GetDomain(description);
                }

                coll.Add(description);
            }

            return coll;
        }

        /// <summary>
        /// Retourne la description des propriétés d'un objet sous forme d'une collection.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="metadataType">Type portant les compléments de description.</param>
        /// <param name="bean">Bean dynamic.</param>
        /// <returns>Description des propriétés.</returns>
        private BeanPropertyDescriptorCollection CreateBeanPropertyCollection(Type beanType, object bean)
        {
            PropertyDescriptor defaultProperty;
            PropertyDescriptorCollection properties;

            if (bean != null && bean is ICustomTypeDescriptor)
            {
                properties = TypeDescriptor.GetProperties(bean);
                defaultProperty = TypeDescriptor.GetDefaultProperty(bean);
            }
            else
            {
                properties = TypeDescriptor.GetProperties(beanType);
                defaultProperty = TypeDescriptor.GetDefaultProperty(beanType);
            }

            return CreateCollection(properties, defaultProperty, beanType);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="bean">Bean.</param>
        /// <returns>Description des propriétés.</returns>
        private BeanDefinition GetDefinitionInternal(Type beanType, object bean)
        {
            Type descriptionType = beanType;

            if (!_beanDefinitionDictionnary.TryGetValue(descriptionType, out BeanDefinition definition))
            {
                TableAttribute table = (TableAttribute)TypeDescriptor.GetAttributes(beanType)[typeof(TableAttribute)];
                string contractName = table?.Name;

                object[] attrs = beanType.GetCustomAttributes(typeof(ReferenceAttribute), false);
                bool isReference = attrs.Length == 1;
                bool isStatic = isReference ? ((ReferenceAttribute)attrs[0]).IsStatic : false;
                BeanPropertyDescriptorCollection properties = CreateBeanPropertyCollection(beanType, bean);

                definition = new BeanDefinition(beanType, properties, contractName, isReference, isStatic);
                if (bean == null && !typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType))
                {
                    _beanDefinitionDictionnary[descriptionType] = definition;
                }
            }

            return definition;
        }

        /// <summary>
        /// Efface la description d'un type.
        /// </summary>
        /// <param name="descriptionType">Type portant la description.</param>
        private void ClearDefinitionCore(Type descriptionType)
        {
            _beanDefinitionDictionnary.Remove(descriptionType);
        }

    }
}
