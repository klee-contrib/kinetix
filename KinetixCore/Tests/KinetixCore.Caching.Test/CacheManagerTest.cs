using System;
using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Kinetix.Caching.Config;
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
    public class CacheManagerTest : DIBaseTest {

        public override void Register()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();

            BuildServiceProvider(serviceCollection);
        }


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

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();
            serviceCollection.AddOptions();
            serviceCollection.Configure<CacheConfig>(cc =>
            {
                CacheConfigItem cci = new CacheConfigItem() { EvictionPolicy = MemoryStoreEvictionPolicy.Lru };
                cc.Caches.Add("CacheDefaultConfiguration", cci);
            });
            BuildServiceProvider(serviceCollection);

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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();
            serviceCollection.AddOptions();
            serviceCollection.Configure<CacheConfig>(cc =>
            {
                CacheConfigItem cci = new CacheConfigItem() { EvictionPolicy = MemoryStoreEvictionPolicy.Lfu };
                cc.Caches.Add("CacheDefaultConfiguration", cci);
            });
            BuildServiceProvider(serviceCollection);

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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();
            serviceCollection.AddOptions();
            serviceCollection.Configure<CacheConfig>(cc =>
            {
                CacheConfigItem cci = new CacheConfigItem() { EvictionPolicy = MemoryStoreEvictionPolicy.Fifo };
                cc.Caches.Add("CacheDefaultConfiguration", cci);
            });
            BuildServiceProvider(serviceCollection);

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
