using System;
using System.Reflection;

namespace Kinetix.Services
{
    /// <summary>
    /// Accesseur sur une méthode.
    /// </summary>
    public class Accessor
    {
        /// <summary>
        /// Nom de l'accesseur.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Contrat.
        /// </summary>
        public Type ContractType { get; set; }

        /// <summary>
        /// Méthode.
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Type de la liste de référence.
        /// </summary>
        public Type ReferenceType { get; set; }
    }
}
