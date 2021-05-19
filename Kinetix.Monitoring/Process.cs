using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Monitoring
{
    /// <summary>
    /// Chaque action génère un événement destiné au monitoring.
    /// Cet événement est évalué selon plusieurs axes correspondant aux compteurs.
    ///
    /// Exemple :
    /// Telle requête aura :
    /// - une taille de page
    /// - un temps de réponse
    /// - un nombre d'accès à la bdd
    /// - un nombre de mails envoyés.
    /// </summary>
    public class Process
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom du processus.</param>
        /// <param name="category">Catégorie de process.</param>
        public Process(string name, string category)
        {
            Id = Guid.NewGuid();
            Name = name;
            Category = category;
        }

        /// <summary>
        /// Id unique du process.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Nom du processus.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Catégorie du processus.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Début du processus.
        /// </summary>
        public DateTime Start { get; } = DateTime.Now;

        /// <summary>
        /// Date de fin du processus.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Process en erreur
        /// </summary>
        public bool IsError { get; internal set; }

        /// <summary>
        /// Durée du processus (en ms).
        /// </summary>
        public int? Duration
        {
            get
            {
                var duration = (End - Start)?.TotalMilliseconds;
                return duration == null
                    ? null :
                    (int?)Math.Round(duration.Value);
            }
        }

        /// <summary>
        /// Durée des sous processus.
        /// </summary>
        public int SubProcessesDuration => SubProcesses.Sum(p => p.Duration.Value);

        /// <summary>
        /// Retourne les compteurs du processus.
        /// </summary>
        internal ConcurrentDictionary<string, int> OwnCounters { get; } = new();

        /// <summary>
        /// Retourne les compteurs totaux du processus, en incluant les sous-processus.
        /// </summary>
        public Dictionary<string, int> Counters
        {
            get
            {
                var counters = new Dictionary<string, int>(OwnCounters);

                foreach (var subprocess in SubProcesses)
                {
                    foreach (var key in subprocess.Counters.Keys)
                    {
                        counters.TryGetValue(key, out var exisitingValue);
                        counters[key] = exisitingValue + subprocess.Counters[key];
                    }
                }

                return counters;
            }
        }

        /// <summary>
        /// Sous processus.
        /// </summary>
        public IList<Process> SubProcesses { get; set; } = new List<Process>();

        /// <summary>
        /// Incrémente le compteur.
        /// </summary>
        /// <param name="counterDefinitionCode">Compteur.</param>
        /// <param name="value">Increment du compteur.</param>
        public void IncrementValue(string counterDefinitionCode, int value)
        {
            OwnCounters.TryGetValue(counterDefinitionCode, out var currentValue);
            OwnCounters[counterDefinitionCode] = currentValue + value;
        }
    }
}
