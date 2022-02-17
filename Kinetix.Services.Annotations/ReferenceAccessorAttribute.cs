namespace Kinetix.Services.Annotations;

/// <summary>
/// Attribut indiquant qu'une méthode permet l'accès à un fichier.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FileAccessorAttribute : Attribute
{
}
