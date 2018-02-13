using System;

namespace Kinetix.Services.Annotations
{
    /// <summary>
    /// Enregistre l'implémentation dans le container d'injection de dépendances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegisterImplAttribute : Attribute
    {
    }
}
