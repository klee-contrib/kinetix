using System.Reflection;

namespace Kinetix.Services;

/// <summary>
/// Accesseur de référence sur une méthode.
/// </summary>
internal class ReferenceAccessor
{
    /// <summary>
    /// Si l'accesseur est asynchrone.
    /// </summary>
    public required bool IsAsync { get; set; }

    /// <summary>
    /// Contrat.
    /// </summary>
    public required Type ContractType { get; set; }

    /// <summary>
    /// Méthode.
    /// </summary>
    public required MethodInfo Method { get; set; }

    /// <summary>
    /// Type de la liste de référence.
    /// </summary>
    public required Type ReferenceType { get; set; }
}
