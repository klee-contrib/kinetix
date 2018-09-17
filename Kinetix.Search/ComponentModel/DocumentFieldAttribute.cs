using System;

namespace Kinetix.Search.ComponentModel
{

    /// <summary>
    /// Attribut de décoration de propriété de document de moteur de recherche.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DocumentFieldAttribute : Attribute
    {

        /// <summary>
        /// Créé une nouvelle instance de DocumentFieldCategory.
        /// </summary>
        /// <param name="category">Catégorie.</param>
        /// <param name="pkOrder">Ordre de la propriété dans la clé primaire composite, si applicable.</param>
        public DocumentFieldAttribute(DocumentFieldCategory category)
        {
            Category = category;
        }

        /// <summary>
        /// Catégorie du champ.
        /// </summary>
        public DocumentFieldCategory Category { get; private set; }

        /// <summary>
        /// Ordre de la propriété dans la clé primaire composite (si applicable).
        /// </summary>
        public int PkOrder { get; set; }
    }
}