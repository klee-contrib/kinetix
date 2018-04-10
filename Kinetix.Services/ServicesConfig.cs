using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kinetix.Services
{
    /// <summary>
    /// Config pour l'enregistrement des services.
    /// </summary>
    public class ServicesConfig
    {
        /// <summary>
        /// Préfixe des assemblies de services à charger.
        /// </summary>
        public string ServiceAssemblyPrefix { get; set; }

        /// <summary>
        /// Assemblies de services.
        /// </summary>
        public ICollection<Assembly> ServiceAssemblies { get; set; }

        /// <summary>
        /// Durée du cache de listes statiques.
        /// </summary>
        public TimeSpan? StaticListCacheDuration { get; set; }

        /// <summary>
        /// Durée du cache de listes de références.
        /// </summary>
        public TimeSpan? ReferenceListCacheDuration { get; set; }
    }
}
