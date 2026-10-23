namespace Kinetix.Services.DependencyInjection;

/// <summary>
/// Désactive l'intercepteur de transaction pour la méthode annotée.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NoTransactionAttribute : Attribute;
