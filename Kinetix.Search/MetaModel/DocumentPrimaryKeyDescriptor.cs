using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Search.MetaModel
{
    /// <summary>
    /// Fournit la description de la clé primaire d'un document.s
    /// </summary>
    public class DocumentPrimaryKeyDescriptor
    {
        private IList<DocumentFieldDescriptor> _fieldDescriptors = new List<DocumentFieldDescriptor>();

        /// <summary>
        /// Ajoute une propriété pour construire la clé primaire.
        /// </summary>
        /// <param name="field">La propriété.</param>
        public void AddProperty(DocumentFieldDescriptor field)
        {
            _fieldDescriptors.Add(field);
            _fieldDescriptors = _fieldDescriptors.OrderBy(a => a.PkOrder).ThenBy(a => a.FieldName).ToList();
        }

        /// <summary>
        /// Récupère la valeur de la clé primaire.
        /// </summary>
        /// <param name="bean">Le bean.</param>
        /// <returns>La clé primaire.</returns>
        public object GetValue(object bean)
        {
            return _fieldDescriptors.Count > 1
                ? string.Join("__", _fieldDescriptors.Select(f => f.GetValue(bean)))
                : _fieldDescriptors.Single().GetValue(bean);
        }
    }
}
