using System.Collections.Generic;

namespace Kinetix.Caching.Config
{
    public class CacheConfig
    {
        public IDictionary<string, CacheConfigItem> Caches { get; set; }
    }
}
