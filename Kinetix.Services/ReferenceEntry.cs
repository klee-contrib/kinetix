using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ComponentModel;

namespace Kinetix.Services
{
    /// <summary>
    /// Entrée pour une liste de référence.
    /// </summary>
    internal class ReferenceEntry
    {
        private readonly IDictionary<object, object> _referenceMap;
        private readonly string _name;

        /// <summary>
        /// Crée une nouvelle entrée pour le type.
        /// </summary>
        /// <param name="referenceList">Liste de référence.</param>
        /// <param name="definition">Définition du bean.</param>
        public ReferenceEntry(ICollection<object> referenceList, BeanDefinition definition)
        {
            _name = definition.BeanType.Name;

            var primaryKey = definition.PrimaryKey;
            if (primaryKey == null)
            {
                throw new NotSupportedException($"Reference type {definition.BeanType.Name} doesn't have a primary key defined. Use the ColumnAttribute to set the primary key property.");
            }

            _referenceMap = referenceList.ToDictionary(r => primaryKey.GetValue(r), r => r);
        }

        /// <summary>
        /// Liste de référence.
        /// </summary>
        public ICollection<object> List => _referenceMap.Values;

        /// <summary>
        /// Retourne un objet de référence.
        /// </summary>
        /// <typeparam name="T">Type souhaité.</typeparam>
        /// <param name="predicate">Prédicat.</param>
        /// <returns>Objet.</returns>
        public T GetReferenceObject<T>(Func<T, bool> predicate)
        {
            return (T)_referenceMap.Single(item => predicate((T)item.Value)).Value;
        }

        /// <summary>
        /// Retourne un objet de référence.
        /// </summary>
        /// <typeparam name="T">Type souhaité.</typeparam>
        /// <param name="primaryKey">Clé primaire.</param>
        /// <returns>Objet.</returns>
        public T GetReferenceObject<T>(object primaryKey)
        {
            /* Cherche la valeur pour la locale demandée. */
            if (_referenceMap.TryGetValue(primaryKey, out var value))
            {
                return (T)value;
            }
            else
            {
                throw new NotSupportedException($"Reference entry {primaryKey} is missing for {_name}.");
            }
        }
    }

    /// <summary>
    /// Cache de listes de référence.
    /// </summary>
    internal class ReferenceCache : Dictionary<string, ReferenceEntry>
    {
    }
}
