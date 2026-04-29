namespace Kinetix.Services.Annotations;

/// <summary>
/// Attribut indiquant qu'une méthode permet l'accès à une
/// liste de reférence.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ReferenceAccessorAttribute : Attribute { }
