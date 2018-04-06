using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Kinetix.Services
{
    /// <summary>
    /// Contrat du manager de référence
    /// <summary>
    public interface IReferenceManager
    {
        /// <summary>
        /// Types de référence disponibles dans le ReferenceManager.
        /// </summary>
        IEnumerable<Type> ReferenceTypes { get; }

        /// <summary>
        /// Vide le cache.
        /// </summary>
        /// <param name="referenceType">Type de la référence.</param>
        /// <param name="referenceName">Nom de la référence.</param>
        void FlushCache(Type referenceType = null, string referenceName = null);

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        ICollection<TReferenceType> GetReferenceList<TReferenceType>(string referenceName = null)
            where TReferenceType : new();

        /// <summary>
        /// Retourne la liste de référence du type demandé.
        /// </summary>
        /// <param name="type">Type de la liste de référence.</typeparam>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        ICollection<object> GetReferenceList(Type type, string referenceName = null);

        /// <summary>
        /// Retourne les éléments de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReference">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        ICollection<TReferenceType> GetReferenceList<TReferenceType>(Func<TReferenceType, bool> predicate, string referenceName = null)
            where TReferenceType : new();

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType à partir d'un objet de critères.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="criteria">Objet contenant les critères.</param>
        /// <returns>Les éléments de la liste qui correspondent aux critères.</returns>
        ICollection<TReferenceType> GetReferenceListByCriteria<TReferenceType>(TReferenceType criteria)
            where TReferenceType : new();

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <returns>Liste de référence.</returns>
        ICollection<TReferenceType> GetReferenceListByPrimaryKeyList<TReferenceType>(params object[] primaryKeyArray)
            where TReferenceType : new();

        /// <summary>
        /// Retourne l'élément unique de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        TReferenceType GetReferenceObject<TReferenceType>(Func<TReferenceType, bool> predicate, string referenceName = null)
            where TReferenceType : new();

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Le type de référence.</returns>
        TReferenceType GetReferenceObjectByPrimaryKey<TReferenceType>(object primaryKey)
            where TReferenceType : new();

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Clef primaire de l'objet.</param>
        /// <param name="propertySelector">Lambda expression de sélection de la propriété.</param>
        /// <returns>Valeur de la propriété sur le bean.</returns>
        string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, Expression<Func<TReferenceType, object>> propertySelector)
            where TReferenceType : new();

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, string defaultPropertyName = null)
            where TReferenceType : new();

        /// <summary>
        /// Parse les différents accésseurs fournis par le type.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        void RegisterAccessors(Type contractType);
    }
}