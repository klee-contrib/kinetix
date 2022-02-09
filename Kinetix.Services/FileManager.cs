using System;
using System.Collections.Generic;
using System.Reflection;
using Kinetix.ComponentModel;
using Kinetix.Services.Annotations;

namespace Kinetix.Services
{
    /// <summary>
    /// Gestionnaire des téléchargemente de fichiers.
    /// </summary>
    public class FileManager : IFileManager
    {
        private readonly IServiceProvider _provider;
        private readonly IDictionary<string, Accessor> _fileAccessors = new Dictionary<string, Accessor>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        public FileManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public DownloadedFile GetFile(string accessorName, int id)
        {
            if (!_fileAccessors.ContainsKey(accessorName))
            {
                throw new ArgumentException($"L'accesseur {accessorName} n'existe pas.");
            }

            var accessor = _fileAccessors[accessorName];
            var service = _provider.GetService(accessor.ContractType);
            return (DownloadedFile)accessor.Method.Invoke(service, new object[] { id });
        }

        /// <summary>
        /// Enregistre les accesseurs de listes de référence une interface.
        /// </summary>
        /// <param name="contractType">Type du contrat d'interface.</param>
        internal void RegisterAccessors(Type contractType)
        {
            foreach (var method in contractType.GetMethods())
            {
                var returnType = method.ReturnType;

                var attribute = method.GetCustomAttribute<FileAccessorAttribute>();
                if (attribute != null)
                {
                    if (returnType != typeof(DownloadedFile))
                    {
                        throw new NotSupportedException($"L'accesseur {method.Name} doit retourner un DownloadedFile.");
                    }

                    if (method.GetParameters().Length != 1)
                    {
                        throw new NotSupportedException($"L'accesseur {method.Name} doit prendre exactement 1 paramètre.");
                    }

                    var accessor = new Accessor
                    {
                        ContractType = contractType,
                        Method = method,
                        Name = method.Name
                    };

                    if (_fileAccessors.ContainsKey(accessor.Name))
                    {
                        throw new NotSupportedException();
                    }

                    _fileAccessors.Add(accessor.Name, accessor);
                }
            }
        }
    }
}