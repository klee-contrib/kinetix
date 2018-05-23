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
        /// Définit le mapping pour la catégorie Facet.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Filter.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapFilter<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie ListFacet.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapListFacet<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
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
        /// Définit le mapping pour la catégorie Search.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapSearch<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
            where TDocument : class;

        /// <summary>
        /// Définit le mapping pour la catégorie Security.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="TDocument">Type du document.</typeparam>
        PropertiesDescriptor<TDocument> MapSecurity<TDocument>(PropertiesDescriptor<TDocument> selector, DocumentFieldDescriptor field)
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
    }
}
