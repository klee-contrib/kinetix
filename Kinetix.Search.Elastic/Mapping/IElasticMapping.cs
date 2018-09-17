using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic.Mapping
{
    /// <summary>
    /// Définit un mapping pour un type de champ, selon sa catégorie.
    /// </summary>
    /// <typeparam name="T">Type du champ pour le mapping.</typeparam>
    public interface IElasticMapping<T> : IElasticMapping
    {
    }

    /// <summary>
    /// Définit un mapping pour un type de champ, selon sa catégorie.
    /// </summary>
    public interface IElasticMapping
    {
        /// <summary>
        /// Définit le mapping pour le type..
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> Map<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;
    }
}
