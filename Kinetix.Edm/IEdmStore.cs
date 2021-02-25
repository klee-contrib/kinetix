using System.Collections.Generic;

namespace Kinetix.Edm
{
    /// <summary>
    /// Contrat des stores de GED.
    /// Les stores encapsulent l'utilisation d'un client qui consomme une implémentation concrète de GED (SharePoint...).
    /// </summary>
    public interface IEdmStore
    {
        /// <summary>
        /// Obtient un document de la GED à partir de son ID.
        /// </summary>
        /// <param name="edmId">ID du document dans la GED.</param>
        /// <returns>Document.</returns>
        EdmDocument Get(object edmId);

        /// <summary>
        /// Charge une liste de documents par leur ID
        /// </summary>
        /// <param name="gedIds">Id des documents dans la GED</param>
        /// <returns></returns>
        Dictionary<int, EdmDocument> GetItems(int[] gedIds);

        /// <summary>
        /// Obtient un document de la GED à partir de son chemin relatif.
        /// </summary>
        /// <param name="path">Le chemin.</param>
        /// <param name="name">Le nom.</param>
        /// <returns>Document.</returns>
        EdmDocument GetByName(string path, string name);

        /// <summary>
        /// Pose un document dans la GED.
        /// </summary>
        /// <param name="document">Document à poser.</param>
        /// <returns>Document avec l'ID renseigné.</returns>
        EdmDocument Put(EdmDocument document);

        /// <summary>
        /// Supprime un document de la GED.
        /// </summary>
        /// <param name="edmId">ID dans la GED.</param>
        void Remove(object edmId);

        /// <summary>
        /// Supprime un document de la GED.
        /// </summary>
        /// <param name="path">Le chemin.</param>
        /// <param name="name">Le nom.</param>
        void RemoveByName(string path, string name);
    }
}
