using System.Collections.Generic;

namespace Kinetix.Search.Config
{
    /// <summary>
    /// Section de configuration du moteur de recherche.
    /// </summary>
    public class SearchConfig
    {
        public Dictionary<string, SearchConfigItem> Servers { get; set; }
    }
}
