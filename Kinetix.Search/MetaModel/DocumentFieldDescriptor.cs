using System;
using System.ComponentModel;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel
{
    /// <summary>
    /// Classe de description d'une propriété.
    /// </summary>
    [Serializable]
    public sealed class DocumentFieldDescriptor
    {
        /// <summary>
        /// Obtient le nom de la propriété.
        /// </summary>
        public string PropertyName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Nom du champ dans le document (camel case).
        /// </summary>
        public string FieldName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Obtient le type de la propriété.
        /// </summary>
        public Type PropertyType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Catégorie de field de document.
        /// </summary>
        public DocumentFieldCategory? DocumentCategory
        {
            get;
            internal set;
        }

        /// <summary>
        /// Catégorie de field de recherche.
        /// </summary>
        public SearchFieldCategory? SearchCategory
        {
            get;
            internal set;
        }

        /// <summary>
        /// Ordre de la propriété dans la clé primaire composite (si applicable).
        /// </summary>
        public int? PkOrder
        {
            get;
            internal set;
        }

        /// <summary>
        /// Retourne la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <returns>Valeur.</returns>
        public object GetValue(object bean)
        {
            var value = TypeDescriptor.GetProperties(bean)[PropertyName].GetValue(bean);
            return value;
        }

        /// <summary>
        /// Définit la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <param name="value">Valeur.</param>
        public void SetValue(object bean, object value)
        {
            var descriptor = TypeDescriptor.GetProperties(bean)[PropertyName];
            descriptor.SetValue(bean, value);
        }

        /// <summary>
        /// Retourne une chaîne de caractère représentant l'objet.
        /// </summary>
        /// <returns>Chaîne de caractère représentant l'objet.</returns>
        public override string ToString()
        {
            return PropertyName;
        }
    }
}
