﻿using Kinetix.Caching.Store;

namespace Kinetix.Caching.Config
{
    /// <summary>
    /// A class to represent Cache configuration.
    /// </summary>
    public class CacheConfigItem
    {
        /// <summary>
        /// Chemin de stockage disque.
        /// </summary>
        public string DiskStorePath { get; set; } = ".";

        /// <summary>
        /// The interval in seconds between runs of the disk expiry thread.
        /// 2 minutes is the default.
        /// </summary>
        public long DiskExpiryThreadIntervalSeconds { get; set; } = 600;

        /// <summary>
        /// For caches that overflow to disk, whether the disk cache persists between CacheManager instances.
        /// </summary>
        public bool DiskPersistent { get; set; } = false;

        /// <summary>
        /// The size of the disk spool used to buffer writes.
        /// </summary>
        public int DiskSpoolBufferSizeMB { get; set; } = 2;

        /// <summary>
        /// The maximum objects to be held in the MemoryStore.
        /// </summary>
        public int MaxElementsInMemory { get; set; } = 1000;

        /// <summary>
        /// The maximum objects to be held in the DiskStore.
        /// </summary>
        public int MaxElementsOnDisk { get; set; } = 10000;

        /// <summary>
        /// The policy used to evict elements from the MemoryStore.
        /// </summary>
        public MemoryStoreEvictionPolicy EvictionPolicy { get; set; } = MemoryStoreEvictionPolicy.Lru;

        /// <summary>
        /// Sets whether elements are eternal. If eternal,  timeouts are ignored and the element
        /// is never expired.
        /// </summary>
        public bool IsEternal { get; set; } = false;

        /// <summary>
        /// The time to idle for an element before it expires. Is only used
        /// if the element is not eternal.A value of 0 means do not check for idling.
        /// </summary>
        public long TimeToIdleSeconds { get; set; } = 0;

        /// <summary>
        /// Sets the time to idle for an element before it expires. Is only used
        /// if the element is not eternal. This attribute is optional in the configuration.
        /// A value of 0 means do not check time to live.
        /// </summary>
        public long TimeToLiveSeconds { get; set; } = 120;

        /// <summary>
        /// Whether elements can overflow to disk when the in-memory cache
        /// has reached the set limit.
        /// </summary>
        public bool IsOverflowToDisk { get; set; } = false;
    }
}
