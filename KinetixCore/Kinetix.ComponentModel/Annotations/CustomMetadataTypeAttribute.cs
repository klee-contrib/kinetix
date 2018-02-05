using System;

namespace Kinetix.ComponentModel.Annotations
{
    /// <summary>
    /// Attribut permettant de spécifier le porteur de méta-données.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomMetadataTypeAttribute : Attribute
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="metadataClassType">Type portant les méta-données.</param>
        /// <exception cref="System.ArgumentNullException">Si type vaut <code>Null</code>.</exception>
        public CustomMetadataTypeAttribute(Type metadataClassType)
        {
            this.MetadataClassType = metadataClassType ?? throw new ArgumentNullException("metadataClassType");
        }

        /// <summary>
        /// Type portant les méta-données.
        /// </summary>
        public Type MetadataClassType
        {
            get;
            private set;
        }
    }
}
