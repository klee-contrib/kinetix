namespace Kinetix.Services.DependencyInjection;

/// <summary>
/// Désactive toutes les traces OpenTelemetry pour la méthode annotée (en utilisant l'intercepteur de traces de service).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NoTraceAttribute : Attribute;
