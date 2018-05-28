using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel
{
    /// <summary>
    /// Fournit la description d'un document.
    /// </summary>
    public class DocumentDescriptor
    {
        private readonly Dictionary<Type, DocumentDefinition> _beanDefinitionDictionnary;

        /// <summary>
        /// Crée un nouvelle instance.
        /// </summary>
        public DocumentDescriptor()
        {
            _beanDefinitionDictionnary = new Dictionary<Type, DocumentDefinition>();
        }

        /// <summary>
        /// Retourne la definition d'un document.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Description des propriétés.</returns>
        public DocumentDefinition GetDefinition(Type beanType)
        {
            if (beanType == null)
            {
                throw new ArgumentNullException("beanType");
            }

            return GetDefinitionInternal(beanType);
        }

        /// <summary>
        /// Crée la collection des descripteurs de propriétés.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Collection.</returns>
        private DocumentFieldDescriptorCollection CreateCollection(Type beanType)
        {
            var coll = new DocumentFieldDescriptorCollection(beanType);
            var properties = TypeDescriptor.GetProperties(beanType);

            foreach (PropertyDescriptor property in properties)
            {
                SearchFieldAttribute fieldAttr = (SearchFieldAttribute)property.Attributes[typeof(SearchFieldAttribute)];
                if (fieldAttr == null)
                {
                    throw new NotSupportedException("Missing SearchFieldAttribute on property " + beanType + "." + property.Name);
                }

                var category = fieldAttr.Category;

                string fieldName = ToCamelCase(property.Name);
                var description = new DocumentFieldDescriptor(
                    property.Name,
                    fieldName,
                    property.PropertyType,
                    category);

                coll[description.PropertyName] = description;
            }

            return coll;
        }

        /// <summary>
        /// Convertit une chaîne en camelCase.
        /// </summary>
        /// <param name="raw">Chaîne source.</param>
        /// <returns>Chaîne en camelCase.</returns>
        private string ToCamelCase(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            return char.ToLower(raw[0]) + raw.Substring(1);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Description des propriétés.</returns>
        private DocumentDefinition GetDefinitionInternal(Type beanType)
        {
            if (!_beanDefinitionDictionnary.TryGetValue(beanType, out DocumentDefinition definition))
            {
                var documentType = (SearchDocumentTypeAttribute)TypeDescriptor.GetAttributes(beanType)[typeof(SearchDocumentTypeAttribute)];
                if (documentType == null)
                {
                    throw new NotSupportedException("Missing SearchDocumentTypeAttribute on type " + beanType);
                }

                var documentTypeName = documentType.DocumentTypeName;

                var properties = CreateCollection(beanType);
                definition = new DocumentDefinition(beanType, properties, documentTypeName);
                _beanDefinitionDictionnary[beanType] = definition;
            }

            return definition;
        }
    }
}
