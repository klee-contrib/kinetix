using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;

namespace Kinetix.Edm.SharePoint
{
    /// <summary>
    /// Store de GED pour SharePoint.
    /// </summary>
    public class SharePointStore : IEdmStore
    {
        /// <summary>
        /// Fichier vide.
        /// </summary>
        private static readonly byte[] EmptyFile = new byte[] { 0 };

        private readonly string _dataSourceName;
        private readonly EdmAnalytics _edmAnalytics;
        private readonly ILogger<SharePointStore> _logger;
        private readonly SharePointManager _sharePointManager;

        /// <summary>
        /// Créé une nouvelle instance de SharePointStore.
        /// </summary>
        /// <param name="dataSourceName">Datasource de la GED.</param>
        /// <param name="sharePointManager">Manager sharepoint.</param>
        /// <param name="edmAnalytics">Analytics.</param>
        /// <param name="logger">Logger.</param>
        public SharePointStore(string dataSourceName, SharePointManager sharePointManager, EdmAnalytics edmAnalytics, ILogger<SharePointStore> logger)
        {
            _dataSourceName = dataSourceName;
            _edmAnalytics = edmAnalytics;
            _logger = logger;
            _sharePointManager = sharePointManager;
        }

        private string LibraryName => _sharePointManager.GetLibrary(_dataSourceName);

        /// <inheritdoc cref="IEdmStore.Get" />
        public EdmDocument Get(object edmId)
        {
            _logger.LogInformation("SharePoint GET " + edmId);
            _edmAnalytics.StartQuery("Sharepoint.Get");

            try
            {
                using var client = GetClient();

                /* L'ID est relatif à la bibliothèque. */
                var id = (int)edmId;

                /* 1. Charge l'item du fichier. */
                var item = client
                    .Web
                    .Lists
                    .GetByTitle(LibraryName)
                    .GetItemById(id);
                client.Load(item);

                /* 2. Exécute la requête. */
                client.ExecuteQuery();

                /* 3. Télécharge le fichier. */
                var relativeUrl = (string)item["FileRef"];
                var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(client, relativeUrl);
                var content = ReadByteArray(fileInfo.Stream);

                /* Retourne le document. */
                return new EdmDocument
                {
                    Content = content
                };
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <inheritdoc cref="IEdmStore.GetByName" />
        public EdmDocument GetByName(string path, string name)
        {
            _logger.LogInformation("SharePoint GET " + name);
            _edmAnalytics.StartQuery("Sharepoint.GetByName");

            try
            {
                using var client = GetClient();

                var list = client
                    .Web
                    .Lists
                    .GetByTitle(LibraryName);

                client.Load(list, l => l.RootFolder.ServerRelativeUrl);

                client.ExecuteQuery();

                var camlQuery = new CamlQuery
                {
                    ViewXml = $@"
                        <View Scope='RecursiveAll'>
                            <Query>
                                <Where>
                                    <And>
                                        <Contains>
                                            <FieldRef Name='FileDirRef' />
                                            <Value Type='Text'>{LibraryName}/{path.Trim('/')}</Value>
                                        </Contains>
                                        <Eq>
                                            <FieldRef Name='FileLeafRef' />
                                            <Value Type='Text'>{name}</Value>
                                        </Eq>
                                    </And>
                                </Where>
                            </Query>
                        </View>
                    "
                };

                var items = list.GetItems(camlQuery);
                client.Load(items, l => l.IncludeWithDefaultProperties(i => i.Folder, i => i.File, i => i.DisplayName));
                client.ExecuteQuery();

                var item = items.FirstOrDefault();
                if (item == null)
                {
                    throw new ArgumentException("Le fichier n'a pas été trouvé dans SharePoint.");
                }

                /* 3. Télécharge le fichier. */
                var relativeUrl = (string)item["FileRef"];
                var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(client, relativeUrl);
                var content = ReadByteArray(fileInfo.Stream);

                /* Retourne le document. */
                return new EdmDocument
                {
                    Content = content
                };
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <inheritdoc cref="IEdmStore.GetItems" />sdha
        public Dictionary<int, EdmDocument> GetItems(int[] gedIds)
        {
            _logger.LogInformation("SharePoint GET (" + gedIds.Length + " items)");
            _edmAnalytics.StartQuery("Sharepoint.GetItems");

            try
            {
                using var client = GetClient();

                var list = client
                    .Web
                    .Lists
                    .GetByTitle(LibraryName);

                var filter = string.Concat(gedIds.Select(id =>
                    $@"<Value Type='Number'>{id}</Value>")
                    .ToArray());

                var camlQuery = new CamlQuery
                {
                    ViewXml = $@"
                        <View Scope='RecursiveAll'>
                            <Query>
                                <Where>
                                    <In><FieldRef Name='ID' />
                                        <Values>
                                            {filter}   
                                        </Values>
                                    </In>
                                </Where>
                            </Query>
                        </View>
                    "
                };

                var items = list.GetItems(camlQuery);
                client.Load(items);


                /* 2. Exécute la requête. */
                client.ExecuteQuery();

                if (items.Count == 0)
                {
                    throw new ArgumentException("Aucun fichier présent dans sharepoint");
                }

                var result = new Dictionary<int, EdmDocument>();
                Parallel.ForEach(items, item =>
                {
                    try
                    {
                        /* 3. Télécharge le fichier. */

                        var relativeUrl = (string)item["FileRef"];
                        var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(client, relativeUrl);
                        var content = ReadByteArray(fileInfo.Stream);

                        /* Retourne le document. */
                        result.Add(item.Id, new EdmDocument
                        {
                            Content = content
                        });
                    }
                    catch (Exception e)
                    {
                        _edmAnalytics.CountError();
                        _logger.LogError($"Téléchargement sharepoint en échec : {item.DisplayName}");
                    }
                });

                return result;
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <inheritdoc cref="IEdmStore.Put" />
        public EdmDocument Put(EdmDocument document)
        {
            _logger.LogInformation("SharePoint PUT " + document.Name);
            _edmAnalytics.StartQuery("Sharepoint.Put");

            /* L'utilisation de SaveBinaryDirect permet de dépasser la limite d'upload de 2Mo de SharePoint. */
            /* Toutefois elle ne fournit pas d'API pour les métadonnées et ne donne pas l'ID de fichier.
             * C'est pourquoi on insert d'abord un fichier vide avec l'API standard, puis on écrase le fichier
             * avec SaveBinaryDirect. */

            try
            {
                using var context = GetClient();

                /* 1. Création d'un fichier vide. */
                var applicationRelativeFileUrl = $"{LibraryName}/{document.Name}";
                var newFile = new FileCreationInformation
                {
                    Content = EmptyFile,
                    Url = applicationRelativeFileUrl,
                    Overwrite = true
                };

                /* 2. Ajout du fichier dans la library. */
                var library = context
                    .Web
                    .Lists
                    .GetByTitle(LibraryName);

                var uploadFile = library
                    .RootFolder
                    .Files
                    .Add(newFile);

                /* 3. Ajout des métadonnées. */
                var item = uploadFile.ListItemAllFields;
                if (document.Fields.Any())
                {
                    foreach (var field in document.Fields)
                    {
                        item[field.Key.ToString()] = field.Value;
                    }

                    item.Update();
                }

                /* 4. Demande le retour de l'ID et de l'URL de la library. */
                context.Load(item, x => x.Id);
                context.Load(library.RootFolder, x => x.ServerRelativeUrl);

                /* 5. Exécute la requête. */
                context.ExecuteQuery();

                /* 6. Récupère l'ID SharePoint du fichier. */
                document.EdmId = item.Id;

                /* 7. Upload le vrai fichier. */
                var serverRelativeLibraryUrl = library.RootFolder.ServerRelativeUrl;
                var serverRelativeFileUrl = $"{serverRelativeLibraryUrl}/{document.Name}";
                using (var stream = new MemoryStream(document.Content))
                {
                    Microsoft.SharePoint.Client.File.SaveBinaryDirect(
                        context,
                        serverRelativeFileUrl,
                        stream,
                        true);
                }

                return document;
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <inheritdoc cref="IEdmStore.Remove" />
        public void Remove(object edmId)
        {
            _logger.LogInformation("SharePoint REMOVE " + edmId);
            _edmAnalytics.StartQuery("Sharepoint.Remove");

            try
            {
                using var client = GetClient();

                /* L'ID est relatif à la bibliothèque. */
                var id = (int)edmId;

                /* 1. Charge l'item du fichier à supprimer. */
                var item = client
                    .Web
                    .Lists
                    .GetByTitle(LibraryName)
                    .GetItemById(id);

                /* 2. Supprime l'objet. */
                item.DeleteObject();

                /* 3. Exécute la requête. */
                try
                {
                    client.ExecuteQuery();
                }
                catch (ServerException)
                {
                    // Si le document existe pas, (•_•) (•_•)>⌐■-■ (⌐■_■)
                    _logger.LogInformation("SharePoint REMOVE " + edmId + " : file doesn't exist");
                }
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <inheritdoc cref="IEdmStore.RemoveByName" />
        public void RemoveByName(string path, string name)
        {
            _logger.LogInformation("SharePoint REMOVE " + name);
            _edmAnalytics.StartQuery("Sharepoint.RemoveByName");

            try
            {
                using var client = GetClient();

                var list = client
                    .Web
                    .Lists
                    .GetByTitle(LibraryName);

                client.Load(list, l => l.RootFolder.ServerRelativeUrl);

                client.ExecuteQuery();

                var camlQuery = new CamlQuery
                {
                    ViewXml = $@"
                        <View Scope='RecursiveAll'>
                            <Query>
                                <Where>
                                    <And>
                                        <Contains>
                                            <FieldRef Name='FileDirRef' />
                                            <Value Type='Text'>{LibraryName}/{path.Trim('/')}</Value>
                                        </Contains>
                                        <Eq>
                                            <FieldRef Name='FileLeafRef' />
                                            <Value Type='Text'>{name}</Value>
                                        </Eq>
                                    </And>
                                </Where>
                            </Query>
                        </View>
                    "
                };

                var items = list.GetItems(camlQuery);
                client.Load(items, l => l.IncludeWithDefaultProperties(i => i.Folder, i => i.File, i => i.DisplayName));
                client.ExecuteQuery();

                var item = items.FirstOrDefault();
                if (item == null)
                {
                    throw new ArgumentException("Le fichier n'a pas été trouvé dans SharePoint.");
                }

                item.DeleteObject();
                client.ExecuteQuery();
            }
            catch
            {
                _edmAnalytics.CountError();
                throw;
            }
            finally
            {
                _edmAnalytics.StopQuery();
            }
        }

        /// <summary>
        /// Lit un byte array dans un stream.
        /// </summary>
        /// <param name="input">Stream.</param>
        /// <returns>Byte array.</returns>
        private static byte[] ReadByteArray(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Obtient le client SharePoint.
        /// </summary>
        /// <returns>Client SharePoint.</returns>
        private ClientContext GetClient()
        {
            return _sharePointManager.ObtainClient(_dataSourceName);
        }
    }
}
