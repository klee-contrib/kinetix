namespace Kinetix.Search.Core;

/// <summary>
/// DocumentLoader abstract pour des documents correpondant à une liste statique.
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public abstract class DocumentLoaderStatic<TDocument> : IDocumentLoader<TDocument>
{
    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(TDocument)" />
    public TDocument Get(TDocument bean)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get(int)" />
    public TDocument Get(int id)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAll" />
    public abstract IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{TDocument})" />
    public IEnumerable<TDocument> GetMany(IEnumerable<TDocument> beans)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany(IEnumerable{int})" />
    public IEnumerable<TDocument> GetMany(IEnumerable<int> ids)
    {
        throw new NotSupportedException();
    }
}
