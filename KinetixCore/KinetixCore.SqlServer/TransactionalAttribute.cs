using System;

namespace KinetixCore.SqlServer
{
    /// <summary>
    /// Déclare la classe (le service) comme étant transactionnelle
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TransactionalAttribute : Attribute
    {
    }
}
