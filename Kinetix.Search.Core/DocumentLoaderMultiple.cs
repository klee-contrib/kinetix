namespace Kinetix.Search.Core;

/// <summary>
/// DocumentLoader abstract pour des documents avec une clé primaire multiple.
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public abstract class DocumentLoaderMultiple<TDocument> : IDocumentLoader<TDocument>
{
    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(int)" />
    public TDocument Get(int id)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(TDocument)" />
    public abstract TDocument Get(TDocument bean);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAll" />
    public abstract IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{int})" />
    public IEnumerable<TDocument> GetMany(IEnumerable<int> ids)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{TDocument})" />
    public abstract IEnumerable<TDocument> GetMany(IEnumerable<TDocument> beans);
}
