namespace Kinetix.ComponentModel.Annotations;

/// <summary>
/// Type C# associé au domaine.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DomainTypeAttribute : Attribute
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="type">Type.</param>
    public DomainTypeAttribute(Type type)
    {
        Type = type;
    }

    /// <summary>
    /// Type.
    /// </summary>
    public Type Type { get; }
}
