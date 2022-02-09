using System.Collections.Generic;

namespace Kinetix.Monitoring
{
    /// <summary>
    /// Description d'une piste d'analyse.
    /// </summary>
    public interface IAnalytics
    {
        /// <summary>
        /// Nom de la catégorie.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Compteurs associés.
        /// </summary>
        IList<Counter> Counters { get; }
    }
}
