using System;

namespace Kinetix.Services.Annotations
{
    /// <summary>
    /// Déclare la classe (le service) comme étant transactionnelle
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TransactionalAttribute : Attribute
    {
    }
}
