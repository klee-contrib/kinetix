using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web
{
    /// <summary>
    /// Contrôleur pour les listes de réference.
    /// </summary>
    public class ReferenceController : Controller
    {
        private readonly IReferenceManager _referenceManager;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="referenceManager">Composant injecté.</param>
        public ReferenceController(IReferenceManager referenceManager)
        {
            _referenceManager = referenceManager;
        }

        /// <summary>
        /// Retourne la liste des listes de références.
        /// </summary>
        /// <returns>La liste.</returns>
        [HttpGet("api/references")]
        public IEnumerable<string> GetReferenceLists()
        {
            return _referenceManager.ReferenceLists;
        }

        /// <summary>
        /// Charge une liste de référénce.
        /// </summary>
        /// <param name="name">Le nom de la liste de reference</param>
        /// <returns>Liste de référence chargée.</returns>
        [HttpGet("api/references/{name}")]
        public ICollection<object> GetReferenceList(string name)
        {
            if (_referenceManager.ReferenceLists.SingleOrDefault(refName => refName == name) == null)
            {
                throw new ArgumentException($"La liste de référence {name} n'existe pas.");
            }

            return _referenceManager.GetReferenceList(name);
        }
    }
}