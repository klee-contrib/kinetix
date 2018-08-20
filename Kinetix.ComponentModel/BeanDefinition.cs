using System;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Définition d'un bean.
    /// </summary>
    [Serializable]
    public class BeanDefinition
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="properties">Collection de propriétés.</param>
        /// <param name="contractName">Nom du contrat (table).</param>
        /// <param name="isReference"><code>True</code> si le bean correspond à une liste de référence, <code>False</code> sinon.</param>
        /// <param name="isStatic"><code>True</code> si le bean correspond à une liste de référence statique, <code>False</code> sinon.</param>
        internal BeanDefinition(Type beanType, BeanPropertyDescriptorCollection properties, string contractName, bool isReference, bool isStatic)
        {
            BeanType = beanType;
            Properties = properties;
            ContractName = contractName;
            IsReference = isReference;
            IsStatic = isStatic;
            foreach (var property in properties)
            {
                if (property.IsPrimaryKey)
                {
                    PrimaryKey = property;
                }

                if (property.IsDefault)
                {
                    DefaultProperty = property;
                }
            }
        }

        /// <summary>
        /// Retourne le type du bean.
        /// </summary>
        public Type BeanType
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom du contrat.
        /// </summary>
        public string ContractName
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne <code>True</code> si le bean est une liste de référence, <code>False</code> sinon.
        /// </summary>
        public bool IsReference
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne <code>True</code> si le Bean est une liste statique, <code>False</code> sinon.
        /// </summary>
        public bool IsStatic
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la propriété par défaut si elle existe.
        /// </summary>
        public BeanPropertyDescriptor DefaultProperty
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la clef primaire si elle existe.
        /// </summary>
        public BeanPropertyDescriptor PrimaryKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la liste des propriétés d'un bean.
        /// </summary>
        public BeanPropertyDescriptorCollection Properties
        {
            get;
            private set;
        }

        /// <summary>
        /// Vérifie les contraintes sur un bean.
        /// </summary>
        /// <param name="bean">Bean à vérifier.</param>
        /// <param name="allowPrimaryKeyNull">True si la clef primaire peut être null (insertion).</param>
        internal void Check(object bean, bool allowPrimaryKeyNull)
        {
            foreach (var property in Properties)
            {
                if (property.DomainName == null || property.IsReadOnly)
                {
                    continue;
                }

                var checkNull = property.IsPrimaryKey ? !allowPrimaryKeyNull : true;
                property.ValidConstraints(property.GetValue(bean), checkNull, null);
            }
        }
    }
}
