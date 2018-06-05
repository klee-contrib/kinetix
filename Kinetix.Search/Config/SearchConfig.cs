using System;
using System.Collections.Generic;
using System.Reflection;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.Config
{
    /// <summary>
    /// Section de configuration du moteur de recherche.
    /// </summary>
    public class SearchConfig
    {
        public Dictionary<string, SearchConfigItem> Servers { get; set; }

        public string GetIndexNameForType(string dataSourceName, Type documentType)
        {
            var connSettings = GetServer(dataSourceName);
            var attribute = documentType.GetCustomAttribute<SearchDocumentTypeAttribute>();
            return $"{connSettings.IndexName}_{attribute.DocumentTypeName}";
        }

        public SearchConfigItem GetServer(string dataSourceName)
        {
            if (!Servers.TryGetValue(dataSourceName, out var server))
            {
                throw new ArgumentException($@"Le server de recherche ""{dataSourceName}"" est introuvable dans la configuration.");
            }

            return server;
        }
    }
}
