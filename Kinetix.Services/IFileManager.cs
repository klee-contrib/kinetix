using Kinetix.ComponentModel;

namespace Kinetix.Services
{
    /// <summary>
    /// Manager pour le téléchargement de fichier.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Télécharge un fichier.
        /// </summary>
        /// <param name="accessorName">Nom de l'accesseur.</param>
        /// <param name="id">Id de l'objet.</param>
        /// <returns>Fichier</returns>
        DownloadedFile GetFile(string accessorName, int id);
    }
}