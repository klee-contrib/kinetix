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
        /// Définit le mapping pour la catégorie FullText.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapFullText<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Result.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapResult<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Sort.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapSort<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Term.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapTerm<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Terms.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapTerms<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;
    }
}
