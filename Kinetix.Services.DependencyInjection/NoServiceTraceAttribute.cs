namespace Kinetix.Services.DependencyInjection;

/// <summary>
/// Désactive l'intercepteur de trace de service pour la méthode annotée.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NoServiceTraceAttribute : Attribute;
