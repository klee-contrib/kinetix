using System;

namespace Kinetix.ComponentModel.Annotations
{
    /// <summary>
    /// Attribut de description de typage d'une association.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ReferencedTypeAttribute : Attribute
    {
        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="referenceType">Type référencé.</param>
        public ReferencedTypeAttribute(Type referenceType)
        {
            ReferenceType = referenceType ?? throw new ArgumentNullException("referenceType");
        }

        /// <summary>
        /// Obtient le type de l'objet de référence associé à la propriété.
        /// </summary>
        public Type ReferenceType
        {
            get;
            private set;
        }
    }
}
