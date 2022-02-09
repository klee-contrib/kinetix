using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.SharePoint.Client;

namespace Kinetix.Edm.SharePoint
{
    /// <summary>
    /// Manager pour la gestion d'SharePoint Edm.
    /// </summary>
    public class SharePointManager
    {
        private readonly IEnumerable<EdmSettings> _connectionSettings;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="connectionSettings">Connection settings.</param>
        public SharePointManager(IEnumerable<EdmSettings> connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        /// <summary>
        /// Récupère le nom de la librarie associée à la datasource.
        /// </summary>
        /// <param name="dataSourceName">Datasource.</param>
        /// <returns>Librarie.</returns>
        public string GetLibrary(string dataSourceName)
        {
            return _connectionSettings.Single(c => c.Name == dataSourceName).Library;
        }

        /// <summary>
        /// Obtient un client SharePointEdm pour une datasource donnée.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <returns>Client SharePoint.</returns>
        public ClientContext ObtainClient(string dataSourceName)
        {
            var connSettings = _connectionSettings.Single(c => c.Name == dataSourceName);
            return new ClientContext(connSettings.Url)
            {
                /* Authentification Windows. */
                Credentials = CredentialCache.DefaultNetworkCredentials
            };
        }
    }
}
