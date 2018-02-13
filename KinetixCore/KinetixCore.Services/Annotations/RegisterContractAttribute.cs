using System;

namespace Kinetix.Services.Annotations
{
    /// <summary>
    /// Enregistre l'interface avec son implémentation dans le container d'injection de dépendances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class RegisterContractAttribute : Attribute
    {
    }
}
