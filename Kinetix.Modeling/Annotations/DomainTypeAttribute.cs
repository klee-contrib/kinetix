namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Type C# associé au domaine.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="type">Type.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DomainTypeAttribute(Type type) : Attribute
{
    /// <summary>
    /// Type.
    /// </summary>
    public Type Type { get; } = type;
}
