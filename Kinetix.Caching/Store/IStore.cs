namespace Kinetix.Caching.Store
{
    /// <summary>
    /// This is the interface for all stores. A store is a physical counterpart to a cache, which
    /// is a logical concept.
    /// </summary>
    internal interface IStore
    {
        /// <summary>
        /// A check to see if a key is in the Store.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>True un élément est associé à cette clef.</returns>
        bool ContainsKey(object key);

        /// <summary>
        /// Prepares for shutdown.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Flush elements to persistent store.
        /// </summary>
        void Flush();

        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>Element.</returns>
        Element Get(object key);

        /// <summary>
        /// Gets an Element from the Disk Store, without updating statistics.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>Element.</returns>
        Element GetQuiet(object key);

        /// <summary>
        /// Puts an item into the cache.
        /// </summary>
        /// <param name="element">Element to put.</param>
        void Put(Element element);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">Clef de l'élément.</param>
        /// <returns>Elément supprimé.</returns>
        Element Remove(object key);

        /// <summary>
        /// Remove all of the elements from the store.
        /// </summary>
        /// <remarks>
        /// If there are registered CacheEventListeners they are notified of the expiry or removal
        /// of the Element as each is removed.
        /// </remarks>
        void RemoveAll();
    }
}
