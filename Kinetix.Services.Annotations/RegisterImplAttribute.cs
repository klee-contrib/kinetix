using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services.Annotations;

/// <summary>
/// Enregistre l'implémentation dans le container d'injection de dépendances.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterImplAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : Attribute
{
    public ServiceLifetime Lifetime { get; private set; } = lifetime;
}
