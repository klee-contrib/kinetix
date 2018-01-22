using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ComponentModel;

namespace Kinetix.Services
{
    /// <summary>
    /// Entrée pour une liste de référence.
    /// </summary>
    /// <typeparam name="T">Type de la liste de référence.</typeparam>
    internal class ReferenceEntry<T>
        where T : new()
    {
        private readonly IDictionary<object, T> _referenceMap = new Dictionary<object, T>();

        /// <summary>
        /// Crée une nouvelle entrée pour le type.
        /// </summary>
        /// <param name="referenceList">Liste de référence.</param>
        /// <param name="definition">Définition du bean.</param>
        public ReferenceEntry(ICollection<T> referenceList, BeanDefinition definition)
        {
            var primaryKey = definition.PrimaryKey;
            if (primaryKey == null)
            {
                throw new NotSupportedException("Reference type " + typeof(T).FullName + " doesn't have a primary key defined. Use the ColumnAttribute to set the primary key property.");
            }

            _referenceMap = referenceList.ToDictionary(r => primaryKey.GetValue(r), r => r);
        }

        /// <summary>
        /// Liste de référence.
        /// </summary>
        public ICollection<T> List => _referenceMap.Select(item => item.Value).ToList();

        /// <summary>
        /// Retourne un object de référence.
        /// </summary>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Objet.</returns>
        public T GetReferenceValue(object primaryKey)
        {
            /* Cherche la valeur pour la locale demandée. */
            if (_referenceMap.TryGetValue(primaryKey, out var value))
            {
                return value;
            }
            else
            {
                throw new NotSupportedException("Reference entry " + primaryKey + " is missing for " + typeof(T) + ".");
            }
        }
    }
}
