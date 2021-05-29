using Kinetix.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web
{
    public static class DownloadedFileExtensions
    {
        /// <summary>
        /// Renvoie un fichier dans la réponse.
        /// </summary>
        /// <param name="file">Fichier à renvoyer.</param>
        /// <returns>Résultat.</returns>
        public static FileResult ToFileResult(this DownloadedFile file)
        {
            return new FileContentResult(file.Fichier, file.ContentType) { FileDownloadName = file.FileName };
        }
    }
}
