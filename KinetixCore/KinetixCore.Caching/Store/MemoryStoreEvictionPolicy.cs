namespace Kinetix.Caching.Store
{
    /// <summary>
    /// The policy used to evict elements from the MemoryStore.
    /// The default value is LRU.
    /// </summary>
    public enum MemoryStoreEvictionPolicy
    {

        /// <summary>
        /// LRU - least recently used.
        /// </summary>
        Lru,

        /// <summary>
        /// LFU - least frequently used.
        /// </summary>
        Lfu,

        /// <summary>
        /// FIFO - first in first out, the oldest element by creation time.
        /// </summary>
        Fifo
    }
}
