using System;
using System.Globalization;
using System.Threading;
using Kinetix.Caching.Config;
using Kinetix.Caching.Store;
using Microsoft.Extensions.Logging;

namespace Kinetix.Caching
{
    /// <summary>
    /// Cache un ensemble d'élément de même nature.
    /// </summary>
    public sealed class Cache : IEhcache, ICacheEventNotificationService, IDisposable
    {
        /// <summary>
        /// A reserved word for cache names. It denotes a default configuration
        /// which is applied to caches created without configuration.
        /// </summary>
        internal const string DefaultCacheName = "default";

        /// <summary>
        /// The default interval between runs of the expiry thread.
        /// </summary>
        internal const long DefaultExpiryThreadIntervalSeconds = 120;

        /// <summary>
        /// Set a buffer size for the spool of approx 30MB.
        /// </summary>
        internal const int DefaultSpoolBufferSize = 30;

        private const MemoryStoreEvictionPolicy DefaultMemoryStoreEvictionPolicy = MemoryStoreEvictionPolicy.Lru;

        /// <summary>
        /// The amount of time to wait if a store gets backed up.
        /// </summary>
        private const int BackOffTimeMillis = 50;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<CacheManager> _logger;

        private DiskStore _diskStore;
        private Status _status;

        /// <summary>
        /// Configuration du cache.
        /// </summary>
        private readonly CacheConfigItem _configuration;
        private readonly string _name;

        /// <summary>
        /// Cache hit count.
        /// </summary>
        private long _hitCount;

        /// <summary>
        /// Memory cache hit count.
        /// </summary>
        private long _memoryStoreHitCount;

        /// <summary>
        /// DiskStore hit count.
        /// </summary>
        private long _diskStoreHitCount;

        /// <summary>
        /// Count of misses where element was not found.
        /// </summary>
        private long _missCountNotFound;

        /// <summary>
        /// Count of misses where element was expired.
        /// </summary>
        private long _missCountExpired;

        /// <summary>
        /// The MemoryStore of this Cache. All caches have a memory store.
        /// </summary>
        private MemoryStore _memoryStore;

        private Guid _guid;

        /// <summary>
        /// Create a new instance
        /// Only the CacheManager can initialise them.
        /// </summary>
        /// <param name="name">The name of the cache. Note that "default" is a reserved name for the defaultCache.</param>
        /// <param name="configuration">Cache description.</param>
        internal Cache(ILogger<CacheManager> logger, string name, CacheConfigItem configuration)
        {
            _logger = logger;
            ChangeStatus(Status.Uninitialized);
            _guid = Guid.NewGuid();
            _configuration = configuration;
            _name = name;
        }

        /// <summary>
        /// Survient quand tous les éléments sont supprimés.
        /// </summary>
        public event EventHandler AllRemoved;

        /// <summary>
        /// Survient lors de l'arrêt du cache.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Survient suite à l'écriture dans le cache disque.
        /// </summary>
        public event EventHandler BackupComplete;

        /// <summary>
        /// Survient quand un élément est retiré du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementEvicted;

        /// <summary>
        /// Survient quand un élément expiré est retiré du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementExpiry;

        /// <summary>
        /// Survient quand un élément est ajouté au cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementPut;

        /// <summary>
        /// Survient quand un élément est supprimé du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementRemoved;

        /// <summary>
        /// Survient quand un élément est mise à jour.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementUpdate;

        /// <summary>
        /// Gets the cache configuration this cache was created with.
        /// </summary>
        public CacheConfigItem Configuration => _configuration;

        /// <summary>
        /// Guid de l'instance du cache.
        /// </summary>
        public Guid CacheGuid => _guid;

        /// <summary>
        /// Does the overflow go to disk.
        /// </summary>
        public bool IsOverflowToDisk => _configuration.IsOverflowToDisk;

        /// <summary>
        /// Gets the maximum number of elements to hold in memory.
        /// </summary>
        public int MaxElementsInMemory => _configuration.MaxElementsInMemory;

        /// <summary>
        /// The policy used to evict elements from the MemoryStore.
        /// The default value is LRU.
        /// </summary>
        public MemoryStoreEvictionPolicy MemoryStoreEvictionPolicy => _configuration.EvictionPolicy;

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Retourne le service de notification.
        /// </summary>
        public ICacheEventNotificationService NotificationService => this;

        /// <summary>
        /// Flushes all cache items from memory to auxilliary caches and close the auxilliary caches.
        /// Should be invoked only by CacheManager.
        /// </summary>
        public void Dispose()
        {
            try
            {
                lock (this)
                {
                    if (_memoryStore != null)
                    {
                        _memoryStore.Dispose();
                        _memoryStore = null;
                    }

                    if (_diskStore != null)
                    {
                        _diskStore.Dispose();
                        _diskStore = null;
                    }

                    _status = Status.Shutdown;
                }

                this.NotificationService.NotifyDisposed(false);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Gets an element from the cache. Updates Element Statistics.
        ///
        /// Note that the Element's lastAccessTime is always the time of this get.
        /// Use GetQuiet(object) to peak into the Element to see its last access time with get.
        /// </summary>
        /// <param name="key">Key a serializable value.</param>
        /// <returns>The element, or null, if it does not exist.</returns>
        public Element Get(object key)
        {
            this.CheckStatus();
            Element element = null;
            lock (this)
            {
                element = this.SearchInMemoryStore(key, true);
                if (element == null && _configuration.IsOverflowToDisk)
                {
                    element = this.SearchInDiskStore(key, true);
                }

                if (element == null)
                {
                    _missCountNotFound++;
                    _logger.LogInformation(_name + " cache - Miss");
                }
                else
                {
                    _hitCount++;
                }
            }

            return element;
        }

        /// <summary>
        /// Gets an element from the cache, without updating Element statistics. Cache statistics are
        /// not updated.
        /// </summary>
        /// <param name="key">Key a serializable value.</param>
        /// <returns>The element, or null, if it does not exist.</returns>
        public Element GetQuiet(object key)
        {
            this.CheckStatus();
            Element element = null;
            lock (this)
            {
                element = this.SearchInMemoryStore(key, false);
                if (element == null && _configuration.IsOverflowToDisk)
                {
                    element = this.SearchInDiskStore(key, false);
                }
            }

            return element;
        }

        /// <summary>
        /// Whether an Element is stored in the cache on Disk, indicating a higher cost of retrieval.
        /// </summary>
        /// <param name="key">True if an element matching the key is found in the diskStore.</param>
        /// <returns>True if an element matching the key is found on disk.</returns>
        public bool IsElementOnDisk(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.CheckStatus();
            if (_configuration.IsOverflowToDisk)
            {
                return _diskStore != null && _diskStore.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Whether an Element is stored in the cache in Memory, indicating a very low cost of retrieval.
        /// </summary>
        /// <param name="key">Element Key.</param>
        /// <returns>True if an element matching the key is found in memory.</returns>
        public bool IsElementInMemory(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.CheckStatus();
            return _memoryStore.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether this cache element has expired.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns>True if it has expired.</returns>
        public bool IsExpired(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            lock (element)
            {
                return element.IsExpired;
            }
        }

        /// <summary>
        /// Put an element in the cache.
        ///
        /// Resets the access statistics on the element, which would be the case if it has previously been
        /// gotten from a cache, and is now being put back.
        ///
        /// Also notifies the CacheEventListener that:
        ///
        /// - the element was put, but only if the Element was actually put.
        /// - if the element exists in the cache, that an update has occurred, even if the element would be expired
        /// if it was requested.
        /// </summary>
        /// <param name="element">An object. If Serializable it can fully participate in replication and the DiskStore.</param>
        public void Put(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            element.ResetAccessStatistics();
            object key = element.Key;
            bool elementExists = IsElementInMemory(key) || IsElementOnDisk(key);
            if (elementExists)
            {
                element.UpdateUpdateStatistics();
            }

            this.ApplyDefaultsToElementWithoutLifespanSet(element);
            this.BackOffIsDiskSpoolFull();

            lock (this)
            {
                _memoryStore.Put(element);
            }

            if (elementExists)
            {
                this.NotificationService.NotifyElementUpdate(element, false);
            }
            else
            {
                this.NotificationService.NotifyElementPut(element, false);
            }
        }

        /// <summary>
        /// Put an element in the cache, without updating statistics, or updating listeners. This is meant to be used
        /// in conjunction with GetQuiet.
        /// </summary>
        /// <param name="element">An object. If Serializable it can fully participate in replication and the DiskStore.</param>
        public void PutQuiet(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            this.ApplyDefaultsToElementWithoutLifespanSet(element);

            lock (this)
            {
                _memoryStore.Put(element);
            }
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        ///
        /// Also notifies the CacheEventListener after the element was removed, but only if an Element
        /// with the key actually existed.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        public bool Remove(object key)
        {
            return this.Remove(key, false, true);
        }

        /// <summary>
        /// Removes all cached items.
        /// </summary>
        public void RemoveAll()
        {
            this.CheckStatus();
            lock (this)
            {
                _memoryStore.RemoveAll();
                if (_configuration.IsOverflowToDisk)
                {
                    _diskStore.RemoveAll();
                }
            }

            this.NotificationService.NotifyRemoveAll(false);
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        public bool RemoveQuiet(object key)
        {
            return this.Remove(key, false, false);
        }

        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool ICacheEventNotificationService.HasElementEvictedListeners()
        {
            return ElementEvicted != null;
        }

        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool ICacheEventNotificationService.HasElementExpiredListeners()
        {
            return ElementExpiry != null;
        }

        /// <summary>
        /// Notifie l'arrêt du cache.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyDisposed(bool remoteEvent)
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifie l'éviction d'un élément.
        /// </summary>
        /// <param name="element">Evicted Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementEvicted(Element element, bool remoteEvent)
        {
            ElementEvicted?.Invoke(this, new CacheEventArgs(element, remoteEvent));
        }

        /// <summary>
        /// Notifie l'expiration d'un élément.
        /// </summary>
        /// <param name="element">Expiry Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementExpiry(Element element, bool remoteEvent)
        {
            ElementExpiry?.Invoke(this, new CacheEventArgs(element, remoteEvent));
        }

        /// <summary>
        /// Notifie l'ajout d'un élément.
        /// </summary>
        /// <param name="element">Added Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementPut(Element element, bool remoteEvent)
        {
            ElementPut?.Invoke(this, new CacheEventArgs(element, remoteEvent));
        }

        /// <summary>
        /// Notifie la suppression d'un élément.
        /// </summary>
        /// <param name="element">Removed Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementRemoved(Element element, bool remoteEvent)
        {
            ElementRemoved?.Invoke(this, new CacheEventArgs(element, remoteEvent));
        }

        /// <summary>
        /// Notifie la suppression de tous les éléments.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyRemoveAll(bool remoteEvent)
        {
            AllRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifie la mise à jour d'un élément.
        /// </summary>
        /// <param name="element">Updated Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementUpdate(Element element, bool remoteEvent)
        {
            ElementUpdate?.Invoke(this, new CacheEventArgs(element, remoteEvent));
        }

        /// <summary>
        /// Notifie la fin de l'écriture dans le cache disque.
        /// </summary>
        void ICacheEventNotificationService.NotifyBackupCompete()
        {
            BackupComplete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Newly created caches do not have a MemoryStore or a DiskStore.
        ///
        /// This method creates those and makes the cache ready to accept elements.
        /// </summary>
        internal void Init()
        {
            lock (this)
            {
                if (_configuration.MaxElementsInMemory == 0)
                {
                    _configuration.MaxElementsInMemory = 1;
                    _logger.LogWarning(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SR.WarnZeroMemoryStoreSize,
                            _name));
                }

                _diskStore = this.CreateDiskStore();
                _memoryStore = MemoryStore.Create(_logger, this, _diskStore);
                this.ChangeStatus(Status.Alive);
            }

            _logger.LogDebug("Initialised cache: " + _name);
        }

        /// <summary>
        /// Setting with Cache defaults.
        /// </summary>
        /// <param name="element">Element to configure.</param>
        private void ApplyDefaultsToElementWithoutLifespanSet(Element element)
        {
            if (!element.IsLifespanSet)
            {
                element.TimeToLive = (int)_configuration.TimeToLiveSeconds;
                element.TimeToIdle = (int)_configuration.TimeToIdleSeconds;
                element.Eternal = _configuration.IsEternal;
            }
        }

        /// <summary>
        /// Wait outside of synchronized block so as not to block readers
        /// If the disk store spool is full wait a short time to give it a chance to
        /// catch up.
        /// </summary>
        private void BackOffIsDiskSpoolFull()
        {
            if (_diskStore != null && _diskStore.BackedUp())
            {
                // back off to avoid OutOfMemoryError
                Thread.Sleep(BackOffTimeMillis);
            }
        }

        /// <summary>
        /// Change cache status.
        /// </summary>
        /// <param name="status">New status.</param>
        private void ChangeStatus(Status status)
        {
            this._status = status;
        }

        /// <summary>
        /// Check cache status.
        /// </summary>
        private void CheckStatus()
        {
            if (!Status.Alive.Equals(_status))
            {
                throw new CacheException(string.Format(
                    CultureInfo.InvariantCulture,
                    SR.ExceptionCacheNotAlive,
                    _name));
            }
        }

        /// <summary>
        /// Creates a disk store.
        /// </summary>
        /// <returns>Disk Store.</returns>
        private DiskStore CreateDiskStore()
        {
            if (_configuration.IsOverflowToDisk)
            {
                return new DiskStore(_logger, this, _configuration.DiskStorePath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Search an object in diskStore.
        /// </summary>
        /// <param name="key">Element Key.</param>
        /// <param name="updateStatistics">True to update statistics.</param>
        /// <returns>Element or null if not found.</returns>
        private Element SearchInDiskStore(object key, bool updateStatistics)
        {
            Element element = null;
            if (updateStatistics)
            {
                element = _diskStore.Get(key);
            }
            else
            {
                element = _diskStore.GetQuiet(key);
            }

            if (element != null)
            {
                if (this.IsExpired(element))
                {
                    _logger.LogDebug(_name + " cache - Disk Store hit, but element expired");

                    _missCountExpired++;
                    this.Remove(key, true, true);
                    element = null;
                }
                else
                {
                    _diskStoreHitCount++;

                    // Put the item back into memory to preserve policies in the memory store and to save updated statistics
                    _memoryStore.Put(element);
                }
            }

            return element;
        }

        /// <summary>
        /// Search an object in memoryStore.
        /// </summary>
        /// <param name="key">Element key.</param>
        /// <param name="updateStatistics">True to update statistics.</param>
        /// <returns>Element or null if not found.</returns>
        private Element SearchInMemoryStore(object key, bool updateStatistics)
        {
            Element element = null;
            if (updateStatistics)
            {
                element = _memoryStore.Get(key);
            }
            else
            {
                element = _memoryStore.GetQuiet(key);
            }

            if (element != null)
            {
                if (this.IsExpired(element))
                {
                    _logger.LogDebug(_name + " cache - Memory cache hit, but element expired");
                    _missCountExpired++;
                    this.Remove(key, true, true);
                    element = null;
                }
                else
                {
                    _memoryStoreHitCount++;
                }
            }

            return element;
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <param name="expiry">If the reason this method is being called is to expire the element.</param>
        /// <param name="notifyListeners">Whether to notify listeners.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        private bool Remove(object key, bool expiry, bool notifyListeners)
        {
            this.CheckStatus();

            bool removed = false;
            Element elementFromMemoryStore = null;
            Element elementFromDiskStore = null;
            lock (this)
            {
                elementFromMemoryStore = _memoryStore.Remove(key);
                if (_configuration.IsOverflowToDisk)
                {
                    elementFromDiskStore = _diskStore.Remove(key);
                }
            }

            if (elementFromMemoryStore != null)
            {
                if (notifyListeners)
                {
                    if (expiry)
                    {
                        this.NotificationService.NotifyElementExpiry(elementFromMemoryStore, false);
                    }
                    else
                    {
                        this.NotificationService.NotifyElementRemoved(elementFromMemoryStore, false);
                    }
                }

                removed = true;
            }

            if (elementFromDiskStore != null)
            {
                if (notifyListeners)
                {
                    if (expiry)
                    {
                        this.NotificationService.NotifyElementExpiry(elementFromDiskStore, false);
                    }
                    else
                    {
                        this.NotificationService.NotifyElementRemoved(elementFromDiskStore, false);
                    }
                }

                removed = true;
            }

            return removed;
        }
    }
}
