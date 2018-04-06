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
        /// Retourne la liste des types de références.
        /// </summary>
        /// <returns>La liste.</returns>
        [HttpGet("api/references")]
        public IEnumerable<string> GetReferenceTypes()
        {
            return _referenceManager.ReferenceTypes.Select(type => type.Name);
        }

        /// <summary>
        /// Charge une liste de référénce.
        /// </summary>
        /// <param name="type">Le nom de classe de la référence.</param>
        /// <param name="name">Le nom de la liste de reference</param>
        /// <returns>Liste de référence chargée.</returns>
        [HttpGet("api/references/{type}")]
        public ICollection<object> GetReferenceList(string type, string name = null)
        {
            var t = _referenceManager.ReferenceTypes.SingleOrDefault(refType => refType.Name == type);

            if (t == null)
            {
                throw new ArgumentException($"Le type {type} n'est pas un type de référence");
            }

            return _referenceManager.GetReferenceList(t, name);
        }
    }
}