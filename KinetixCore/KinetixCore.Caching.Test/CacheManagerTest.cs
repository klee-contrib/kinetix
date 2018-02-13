using System;
using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Kinetix.Caching.Config;
using KinetixCore.Caching.Test;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Kinetix.Caching.Store;

namespace Kinetix.Caching.Test {
    /// <summary>
    /// Classe de test du gestionnaire de cache.
    /// </summary>
    [TestFixture]
    public class CacheManagerTest : AbstractCacheTest {

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void GetCacheTest() {
            var serviceProvider = GetServiceProvider();
            var manager = serviceProvider.GetService<CacheManager>();

            using (Cache cache = manager.GetCache("Test")) {

            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void GetCacheLruTest() {

            RegisterCustomConfig(new CacheConfigItem() {EvictionPolicy = MemoryStoreEvictionPolicy.Lru});

            var serviceProvider = GetServiceProvider();
            var manager = serviceProvider.GetService<CacheManager>();

            using (Cache cache = manager.GetCache("Test")) {
                Assert.AreEqual(MemoryStoreEvictionPolicy.Lru, cache.Configuration.EvictionPolicy);
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetCacheLfuTest() {
            RegisterCustomConfig(new CacheConfigItem() { EvictionPolicy = MemoryStoreEvictionPolicy.Lfu });

            var serviceProvider = GetServiceProvider();
            var manager = serviceProvider.GetService<CacheManager>();

            manager.GetCache("Test");
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetCacheFifoTest() {
            RegisterCustomConfig(new CacheConfigItem() { EvictionPolicy = MemoryStoreEvictionPolicy.Fifo });

            var serviceProvider = GetServiceProvider();
            var manager = serviceProvider.GetService<CacheManager>();

            manager.GetCache("Test");

        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void DefaultMemoryStoreEvictionPolicyTest() {
            var serviceProvider = GetServiceProvider();
            var manager = serviceProvider.GetService<CacheManager>();
            
            using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(MemoryStoreEvictionPolicy.Lru, cache.Configuration.EvictionPolicy);
            }
        }
    }
}
