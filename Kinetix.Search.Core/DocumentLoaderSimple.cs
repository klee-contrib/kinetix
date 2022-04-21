namespace Kinetix.Search.Core;

/// <summary>
/// DocumentLoader abstract pour des documents avec une clé primaire simple.
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public abstract class DocumentLoaderSimple<TDocument> : IDocumentLoader<TDocument>
{
    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(TDocument)" />
    public TDocument Get(TDocument bean)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(int)" />
    public abstract TDocument Get(int id);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAll" />
    public abstract IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{TDocument})" />
    public IEnumerable<TDocument> GetMany(IEnumerable<TDocument> beans)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{int})" />
    public abstract IEnumerable<TDocument> GetMany(IEnumerable<int> ids);
}
