using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Kinetix.ComponentModel.Annotations;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Factory permettant d'instancier un bean fils à partir du bean parent.
    /// </summary>
    /// <typeparam name="TParent">Le bean parent.</typeparam>
    /// <typeparam name="TChild">Le bean fils.</typeparam>
    public class BeanFactory<TParent, TChild>
        where TChild : TParent, new()
        where TParent : class
    {

        private readonly BeanDescriptor _beanDescriptor;

        public BeanFactory(BeanDescriptor beanDescriptor)
        {
            _beanDescriptor = beanDescriptor;
        }

        /// <summary>
        /// Crée un bean héritant du bean d'origine.
        /// </summary>
        /// <param name="bean">Le bean source.</param>
        /// <returns>Le bean héritant initialisé.</returns>
        public TChild CreateBean(TParent bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            var newBean = new TChild();
            FillBean(bean, newBean);
            return newBean;
        }

        /// <summary>
        /// Remplit un bean héritant du bean d'origine.
        /// </summary>
        /// <param name="parentBean">Le bean parent source.</param>
        /// <param name="childBean">Le bean child target que nous remplissions.</param>
        public void FillBean(TParent parentBean, TChild childBean)
        {
            if (parentBean == null)
            {
                throw new ArgumentNullException(nameof(parentBean));
            }

            if (childBean == null)
            {
                throw new ArgumentNullException(nameof(childBean));
            }

            var definitionBean = _beanDescriptor.GetDefinition(parentBean);
            var definitionNewBean = _beanDescriptor.GetDefinition(childBean);

            var isMetadataProvided = typeof(TChild).GetCustomAttributes(typeof(MetadataTypeProviderAttribute), true).Length != 0;

            foreach (var property in definitionBean.Properties)
            {
                if (isMetadataProvided && property.IsReadOnly)
                {
                    var propertyDescriptor = typeof(TChild).GetProperty(property.PropertyName);
                    if (propertyDescriptor.CanWrite)
                    {
                        propertyDescriptor.SetValue(childBean, property.GetValue(parentBean), null);
                    }
                }
                else if (!property.IsReadOnly)
                {
                    definitionNewBean.Properties[property.PropertyName].SetValue(childBean, property.GetValue(parentBean));
                }
                else if (property.PropertyType.IsGenericType && typeof(ICollection<>).Equals(property.PropertyType.GetGenericTypeDefinition()))
                {
                    var values = (IList)property.GetValue(parentBean);
                    var newBeanValues = (IList)property.GetValue(childBean);
                    foreach (var value in values)
                    {
                        newBeanValues.Add(value);
                    }
                }
            }
        }

        /// <summary>
        /// Crée une collection de bean héritant du bean contenu dans la collection.
        /// </summary>
        /// <param name="listeParent">La collection traitée.</param>
        /// <returns>La collection correctement initialisée.</returns>
        public ICollection<TChild> CreateCollection(ICollection<TParent> listeParent)
        {
            if (listeParent == null)
            {
                throw new ArgumentNullException(nameof(listeParent));
            }

            ICollection<TChild> listeChild = new List<TChild>(listeParent.Count);
            CreateCollection(listeParent, listeChild);
            return listeChild;
        }

        /// <summary>
        /// Crée une collection de bean héritant du bean contenu dans la collection.
        /// </summary>
        /// <param name="listeParent">La collection parent à traitée.</param>
        /// <param name="listeChild">La collection correctement initialisée.</param>
        public void CreateCollection(ICollection<TParent> listeParent, ICollection<TChild> listeChild)
        {
            if (listeParent == null)
            {
                throw new ArgumentNullException(nameof(listeParent));
            }

            if (listeChild == null)
            {
                throw new ArgumentNullException(nameof(listeChild));
            }

            foreach (var tmp in listeParent)
            {
                listeChild.Add(CreateBean(tmp));
            }
        }
    }

    /// <summary>
    /// Factory permettant d'instancier un bean en initialisant correctement les collections.
    /// </summary>
    /// <typeparam name="T">Le type d'objet.</typeparam>
    public class BeanFactory<T> : IBeanFactory
        where T : new()
    {
        private static readonly Type[] EmptyConstructorArgs = new Type[0];

        private readonly BeanPropertyDescriptorCollection _properties;
        private readonly PropertyDescriptorCollection _nativeProperties;

        private readonly BeanDescriptor _beanDescriptor;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public BeanFactory(BeanDescriptor beanDescriptor)
        {
            _beanDescriptor = beanDescriptor;
            _properties = _beanDescriptor.GetDefinition(typeof(T), true).Properties;
            _nativeProperties = TypeDescriptor.GetProperties(typeof(T));
        }

        /// <summary>
        /// Reset un bean.
        /// </summary>
        /// <param name="bean">Le bean testé.</param>
        public void ResetBean(T bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException("bean");
            }

            var definitionBean = _beanDescriptor.GetDefinition(bean);
            foreach (var property in definitionBean.Properties)
            {
                if (!property.IsReadOnly)
                {
                    property.SetValue(bean, null);
                }
                else if (property.PropertyType.IsGenericType && typeof(ICollection<>).Equals(property.PropertyType.GetGenericTypeDefinition()))
                {
                    var list = (IList)property.GetValue(bean);
                    list.Clear();
                }
                else if (property.PrimitiveType == null)
                {
                    var factory = CreateBeanFactory(property.PropertyType);
                    factory.ResetBean(property.GetValue(bean));
                }
            }
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        /// <param name="source">Source de données.</param>
        /// <param name="target">Cible de la copie.</param>
        public virtual void CloneBean(T source, T target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            foreach (var property in _properties)
            {
                var prop = _nativeProperties[property.PropertyName];
                if (property.PrimitiveType != null || property.PropertyType.IsEnum)
                {
                    if (!property.IsReadOnly)
                    {
                        var initialValue = prop.GetValue(source);

                        // Si la propriété est un type primitif ou un enum, copie directe.
                        prop.SetValue(target, initialValue);
                    }
                }
                else if (!property.PropertyType.IsGenericType &&
                  property.PropertyType.GetConstructor(EmptyConstructorArgs) != null)
                {
                    var factory = CreateBeanFactory(property.PropertyType);
                    if (prop.IsReadOnly)
                    {
                        var initialValue = prop.GetValue(source);
                        factory.CloneBean(initialValue, prop.GetValue(target));
                    }
                    else
                    {

                        // Si la propriété n'est pas générique et qu'un constructeur publique par défaut existe, recopie par BeanFactory.
                        var initialValue = prop.GetValue(source);
                        prop.SetValue(target, factory.CloneBean(initialValue));
                    }
                }
                else
                {
                    if (IsGenericCollection(property.PropertyType, out var typeFactory, out var innerType))
                    {
                        var valueList = (IList)prop.GetValue(target);
                        if (property.GetValue(source) is ICollection collection && valueList != null)
                        {
                            foreach (var item in collection)
                            {
                                valueList.Add(item);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        /// <param name="bean">Le bean à cloner.</param>
        /// <returns>Le bean cloné.</returns>
        public T CloneBean(T bean)
        {
            var newBean = Activator.CreateInstance<T>();
            if (bean != null)
            {
                CloneBean(bean, newBean);
            }

            return newBean;
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        /// <param name="bean">Le bean à cloner.</param>
        /// <returns>Le bean cloné.</returns>
        object IBeanFactory.CloneBean(object bean)
        {
            return CloneBean((T)bean);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        /// <param name="source">Source de données.</param>
        /// <param name="target">Cible de la copie.</param>
        void IBeanFactory.CloneBean(object source, object target)
        {
            CloneBean((T)source, (T)target);
        }

        /// <summary>
        /// Fixe les valeurs d'un bean a null pour les types primitifs, clear des listes.
        /// </summary>
        /// <param name="bean">Le bean à réinitialiser.</param>
        void IBeanFactory.ResetBean(object bean)
        {
            ResetBean((T)bean);
        }

        /// <summary>
        /// Indique si le type est une collection générique.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="listType">Type d'une liste générique correspondant à la collection.</param>
        /// <param name="innerType">Type contenu dans la liste.</param>
        /// <returns>True si le type est une liste.</returns>
        private static bool IsGenericCollection(Type propertyType, out Type listType, out Type innerType)
        {
            if (propertyType.IsGenericType)
            {
                var generic = propertyType.GetGenericTypeDefinition();
                if (typeof(ICollection<>).Equals(generic))
                {
                    innerType = propertyType.GetGenericArguments()[0];
                    listType = typeof(List<>).MakeGenericType(innerType);
                    return true;
                }
            }

            listType = null;
            innerType = null;
            return false;
        }

        /// <summary>
        /// Crée un BeanFactory associé au type passé en paramètre.
        /// </summary>
        /// <param name="type">Type associé.</param>
        /// <returns>BeanFactory.</returns>
        private static IBeanFactory CreateBeanFactory(Type type)
        {
            var factoryType = typeof(BeanFactory<>).MakeGenericType(type);
            return (IBeanFactory)Activator.CreateInstance(factoryType);
        }
    }
}
