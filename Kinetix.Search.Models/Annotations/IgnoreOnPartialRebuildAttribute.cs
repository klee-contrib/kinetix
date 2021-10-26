using System;

namespace Kinetix.Search.Models.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreOnPartialRebuildAttribute : Attribute
    {
        /// <summary>
        /// Ne supprime pas les documents plus vieux que le nombre de jours demandé lors d'un rebuild partiel.
        /// </summary>
        public int OlderThanDays { get; set; }
    }
}
