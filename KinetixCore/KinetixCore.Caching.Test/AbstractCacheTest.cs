using Kinetix.Caching;
using Kinetix.Caching.Config;
using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace KinetixCore.Caching.Test
{
    public class AbstractCacheTest : DIBaseTest
    {

        public override void Register()
        {
            base.Register();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

            serviceCollection.Configure<CacheConfig>(cc =>
            {
                cc.Caches = new Dictionary<string, CacheConfigItem>();
                cc.Caches.Add("Test", new CacheConfigItem());
            });

            BuildServiceProvider(serviceCollection);
        }

        
        protected void RegisterCustomConfig(CacheConfigItem cci)
        {

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CacheManager>();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

            serviceCollection.Configure<CacheConfig>(cc =>
            {
                cc.Caches = new Dictionary<string, CacheConfigItem>();
                cc.Caches.Add("Test", cci);
            });

            BuildServiceProvider(serviceCollection);
        }


    }
}
