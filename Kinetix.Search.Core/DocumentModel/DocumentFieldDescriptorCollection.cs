﻿using System.Collections;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Collection des descriptions de propriété d'un objet.
/// La clef utilisé est le nom de la propriété.
/// </summary>
[Serializable]
public sealed class DocumentFieldDescriptorCollection : IEnumerable<DocumentFieldDescriptor>
{
    private readonly Type _beanType;
    private readonly Dictionary<string, DocumentFieldDescriptor> _properties;

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <exception cref="System.ArgumentNullException">Si bean type est null.</exception>
    internal DocumentFieldDescriptorCollection(Type beanType)
    {
        _beanType = beanType ?? throw new ArgumentNullException("beanType");
        _properties = new Dictionary<string, DocumentFieldDescriptor>();
    }

    /// <summary>
    /// Retourne la description d'un champ à partir de son nom.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <returns>Description de la propriété.</returns>
    public DocumentFieldDescriptor this[string fieldName]
    {
        get
        {
            try
            {
                return _properties[fieldName];
            }
            catch (KeyNotFoundException e)
            {
                throw new ArgumentException("Champ " + fieldName + " non trouvée pour le type " + _beanType.FullName + ".", e);
            }
        }

        set => _properties[fieldName] = value;
    }

    /// <summary>
    /// Indique si la collection contient une propriété.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété.</param>
    /// <returns><code>True</code> si la collection contient la propriété.</returns>
    public bool HasProperty(string propertyName)
    {
        return _properties.ContainsKey(propertyName);
    }

    /// <summary>
    /// Obtient l'énumérateur.
    /// </summary>
    /// <returns>Enumérateur.</returns>
    public IEnumerator<DocumentFieldDescriptor> GetEnumerator()
    {
        return _properties.Values.GetEnumerator();
    }

    /// <summary>
    /// Obtient l'énumérateur.
    /// </summary>
    /// <returns>Enumérateur.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _properties.Values.GetEnumerator();
    }
}
