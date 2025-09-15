using System.Reflection;
using Kinetix.Modeling;
using Kinetix.Services.Annotations;

namespace Kinetix.Services;

/// <summary>
/// Gestionnaire des téléchargemente de fichiers.
/// </summary>
/// <param name="provider">Service provider.</param>
public class FileManager(IServiceProvider provider) : IFileManager
{
    private readonly Dictionary<string, Accessor> _fileAccessors = [];

    /// <inheritdoc cref="IFileManager.GetFile" />
    public DownloadedFile GetFile(string accessorName, int id)
    {
        if (!_fileAccessors.TryGetValue(accessorName, out var accessor))
        {
            throw new ArgumentException($"L'accesseur {accessorName} n'existe pas.", nameof(accessorName));
        }

        var service = provider.GetService(accessor.ContractType);
        return (DownloadedFile)accessor.Method.Invoke(service, [id]);
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
                    Name = method.Name,
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
