using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel
{
    /// <summary>
    /// Définition d'un bean.
    /// </summary>
    [Serializable]
    public class DocumentDefinition
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="properties">Collection de propriétés.</param>
        /// <param name="documentTypeName">Nom du contrat (table).</param>
        internal DocumentDefinition(Type beanType, DocumentFieldDescriptorCollection properties, string documentTypeName)
        {
            BeanType = beanType;
            Fields = properties;
            DocumentTypeName = documentTypeName;
            foreach (var property in properties)
            {
                switch (property.Category)
                {
                    case SearchFieldCategory.Id:
                        PrimaryKey.AddProperty(property);
                        break;
                    case SearchFieldCategory.Search:
                        TextFields.Add(property);
                        break;
                    case SearchFieldCategory.Security:
                        SecurityField = property;
                        break;
                    default:
                        break;
                }
            }

            if (properties.Count(prop => prop.Category == SearchFieldCategory.Security) > 1)
            {
                throw new NotSupportedException($"{beanType} has multiple Security fields");
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
        public string DocumentTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la clef primaire si elle existe.
        /// </summary>
        public DocumentPrimaryKeyDescriptor PrimaryKey
        {
            get;
            private set;
        } = new DocumentPrimaryKeyDescriptor();

        /// <summary>
        /// Retourne les propriétés de recherche textuelle.
        /// </summary>
        public ICollection<DocumentFieldDescriptor> TextFields
        {
            get;
            private set;
        } = new List<DocumentFieldDescriptor>();

        /// <summary>
        /// Retourne la propriété de filtrage de sécurité.
        /// </summary>
        public DocumentFieldDescriptor SecurityField
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la liste des propriétés d'un bean.
        /// </summary>
        public DocumentFieldDescriptorCollection Fields
        {
            get;
            private set;
        }
    }
}
