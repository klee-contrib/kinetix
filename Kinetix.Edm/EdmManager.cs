﻿using System;
using System.Collections.Generic;
using Kinetix.Edm.SharePoint;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.Edm
{
    /// <summary>
    /// Manager pour les stores de GED.
    /// </summary>
    public sealed class EdmManager : IEdmManager
    {
        private readonly Dictionary<string, IEdmStore> _storeMap = new();
        private readonly string[] _dataSourceNames;
        private readonly AnalyticsManager _analytics;
        private readonly ILogger<SharePointStore> _logger;
        private readonly SharePointManager _sharePointManager;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="sharePointManager">Manager sharepoint.</param>
        /// <param name="analytics">Analytics.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dataSourceNames">Sources de données (la première sera la source par défaut).</param>
        public EdmManager(SharePointManager sharePointManager, AnalyticsManager analytics, ILogger<SharePointStore> logger, params string[] dataSourceNames)
        {
            if (dataSourceNames.Length < 1)
            {
                throw new ArgumentException("Au moins une datasource doit être renseignée.");
            }

            _dataSourceNames = dataSourceNames;
            _analytics = analytics;
            _logger = logger;
            _sharePointManager = sharePointManager;
        }

        /// <summary>
        /// Retourne l'instance du store.
        /// </summary>
        /// <param name="dataSourceName">Source de données : source par défaut si nulle.</param>
        /// <returns>Le store.</returns>
        public IEdmStore GetStore(string dataSourceName = null)
        {
            var dsName = dataSourceName;
            if (string.IsNullOrEmpty(dsName))
            {
                dsName = _dataSourceNames[0];
            }

            if (_storeMap.TryGetValue(dsName, out var basicstore))
            {
                return basicstore;
            }

            lock (_storeMap)
            {
                if (_storeMap.TryGetValue(dsName, out basicstore))
                {
                    return basicstore;
                }
                else
                {
                    var store = new SharePointStore(dsName, _sharePointManager, _analytics, _logger);
                    _storeMap.Add(dsName, store);
                    return store;
                }
            }
        }
    }
}
