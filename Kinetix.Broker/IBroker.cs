﻿using System.Collections.Generic;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker
{
    /// <summary>
    /// Interface permettant de manipuler les brokers sans leur type anonyme.
    /// </summary>
    public interface IBroker
    {
    }

    /// <summary>
    /// Interface de définition d'un broker d'accès aux données.
    /// </summary>
    /// <typeparam name="T">Type du bean à manipuler.</typeparam>
    public interface IBroker<T> : IBroker
        where T : new()
    {
        /// <summary>
        /// Supprime un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Clef primaire de l'objet.</param>
        void Delete(object primaryKey);

        /// <summary>
        /// Supprime plusieurs beans à partir de leur clé primaire.
        /// </summary>
        /// <param name="primaryKeys">Clef primaires des objets.</param>
        void DeleteCollection(ICollection<int> primaryKeys);

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="criteria">Critères de suppression.</param>
        void DeleteAllByCriteria(FilterCriteria criteria);

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="bean">Critères de suppression.</param>
        void DeleteAllByCriteria(T bean);

        /// <summary>
        /// Retourne un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        /// <returns>Bean.</returns>
        T Get(object primaryKey);

        /// <summary>
        /// Retourne tous les beans pour un type.
        /// </summary>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        IList<T> GetAll(QueryParameter queryParameter = null);

        /// <summary>
        /// Retourne tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="criteria">Liste des critères.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        IList<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter queryParameter = null);

        /// <summary>
        /// Retourne tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="bean">Bean de critère.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        IList<T> GetAllByCriteria(T bean, QueryParameter queryParameter = null);

        /// <summary>
        /// Retourne un bean à partir de critères de recherches.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean.</returns>
        /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
        T GetByCriteria(FilterCriteria criteria);

        /// <summary>
        /// Retourne un bean à partir de critères de recherches.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean.</returns>
        /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
        T GetByCriteria(T criteria);

        /// <summary>
        /// Sauvegarde un bean.
        /// </summary>
        /// <param name="bean">Bean à enregistrer.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
        /// <returns>Clef primaire de l'objet.</returns>
        object Save(T bean, ColumnSelector columnSelector = null);

        /// <summary>
        /// Sauvegarde l'ensemble des éléments d'une association n-n.
        /// </summary>
        /// <param name="values">Les valeurs à ajouter via associations.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour.</param>
        void SaveAll(ICollection<T> values, ColumnSelector columnSelector = null);

        /// <summary>
        /// Insére l'ensemble des éléments.
        /// </summary>
        /// <param name="values">Valeurs à insérer.</param>
        /// <returns>Valeurs insérées.</returns>
        ICollection<T> InsertAll(ICollection<T> values);

        /// <summary>
        /// Vérifie si l'objet est utilisé.
        /// </summary>
        /// <param name="primaryKey">Clé primaire de l'objet à vérifier.</param>
        /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
        /// <returns>True si l'objet est utilisé.</returns>
        bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null);

        /// <summary>
        /// Vérifie si au moins un objet dans la collection est utilisé.
        /// </summary>
        /// <param name="primaryKeys">Clés primaires des objets à vérifier.</param>
        /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
        /// <returns>True si au moins un objet est utilisé.</returns>
        bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null);
    }
}
