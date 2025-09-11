using System.Reflection;
using Kinetix.Services.DependencyInjection;

namespace Kinetix.Services;

/// <summary>
/// Config pour l'enregistrement des services.
/// </summary>
public class ServicesConfig
{
    /// <summary>
    /// Permet de remplacer les intercepteurs posés par défaut.
    /// </summary>
    public Func<Type, Action<InterceptionOptions>> InterceptionOptions { get; set; }

    /// <summary>
    /// Préfixe d'assembly dans lequel chercher les services à enregistrer.
    /// </summary>
    internal string ServiceAssemblyPrefix { get; set; }

    /// <summary>
    /// Assemblies dans lesquels chercher les services.
    /// </summary>
    internal List<Assembly> ServiceAssemblies { get; } = [];

    /// <summary>
    /// Durée de cache des listes de référence.
    /// </summary>
    internal TimeSpan ReferenceCacheDuration { get; private set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Type a instancier pour notifier des flushs de liste de référence.
    /// </summary>
    internal Type ReferenceNotifier { get; private set; }

    /// <summary>
    /// Enregistre des assemblies à parcourir pour enregistrer des services.
    /// </summary>
    /// <param name="assemblies">Assemblies.</param>
    /// <returns>Config.</returns>
    public ServicesConfig AddAssemblies(params Assembly[] assemblies)
    {
        ServiceAssemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Remplace les intercepteurs posés par défaut, en fonction du type de service.
    /// </summary>
    /// <param name="options">Fonction déterminant les intercepteurs à poser par type.</param>
    /// <returns>Config.</returns>
    public ServicesConfig WithInterceptors(Func<Type, Action<InterceptionOptions>> options)
    {
        InterceptionOptions = options;
        return this;
    }

    /// <summary>
    /// Spécifie la durée du cache des listes de référence.
    /// </summary>
    /// <param name="duration">Durée.</param>
    /// <returns>Config.</returns>
    public ServicesConfig WithReferenceCacheDuration(TimeSpan duration)
    {
        ReferenceCacheDuration = duration;
        return this;
    }

    /// <summary>
    /// Spécifie une implémentation de notifier de flush de liste de référence.
    /// </summary>
    /// <typeparam name="T">Type du notifier.</typeparam>
    /// <returns>Config.</returns>
    public ServicesConfig WithReferenceNotifier<T>()
        where T : IReferenceNotifier
    {
        ReferenceNotifier = typeof(T);
        return this;
    }
}
