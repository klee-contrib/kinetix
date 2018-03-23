using System;
using System.Collections.Generic;
using Kinetix.Caching.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kinetix.Caching
{
    /// <summary>
    /// Gestionnaire de cache.
    /// </summary>
    public sealed class CacheManager : IDisposable
    {
        private readonly Dictionary<string, Cache> _cacheDictionnary = new Dictionary<string, Cache>();
        private readonly CacheConfig _cacheConfig;
        private readonly ILogger<CacheManager> _logger;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="cacheConfig">Config du cache.</param>
        /// <param name="logger">Logger.</param>
        public CacheManager(ILogger<CacheManager> logger, IOptions<CacheConfig> cacheConfig)
        {
            _cacheConfig = cacheConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Retourne le cache. Le cache est créé si nécessaire.
        /// </summary>
        /// <param name="cacheName">Nom du cache.</param>
        /// <returns>Cache.</returns>
        public Cache GetCache(string cacheName)
        {
            lock (this)
            {
                if (!_cacheDictionnary.TryGetValue(cacheName, out Cache cache))
                {
                    cache = CreateCache(cacheName);
                    _cacheDictionnary[cacheName] = cache;
                }

                return cache;
            }
        }

        /// <summary>
        /// Libère les ressources du cache.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        /// <param name="disposing">Dispose.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Cache cache in _cacheDictionnary.Values)
                {
                    cache.Dispose();
                }

                _cacheDictionnary.Clear();
            }
        }

        /// <summary>
        /// Crée un nouveau cache.
        /// </summary>
        /// <param name="cacheName">Nom du cache.</param>
        /// <returns>Cache.</returns>
        private Cache CreateCache(string cacheName)
        {
            if (_cacheConfig != null && _cacheConfig.TryGetValue(cacheName, out var element))
            {
                var cache = new Cache(_logger, cacheName, element);
                cache.Init();
                return cache;
            }
            else
            {
                _logger.LogWarning($"{cacheName} introuvable dans la configuration.");
                return null;
            }
        }
    }
}
