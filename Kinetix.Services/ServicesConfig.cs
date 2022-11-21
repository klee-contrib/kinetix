using System.Reflection;
using Kinetix.Services.DependencyInjection;

namespace Kinetix.Services;

/// <summary>
/// Config pour l'enregistrement des services.
/// </summary>
public class ServicesConfig
{
    internal string ServiceAssemblyPrefix { get; set; }
    internal List<Assembly> ServiceAssemblies { get; } = new List<Assembly>();

    internal TimeSpan ReferenceListCacheDuration { get; private set; } = TimeSpan.FromMinutes(10);
    internal TimeSpan StaticListCacheDuration { get; private set; } = TimeSpan.FromHours(1);
    internal Type ReferenceNotifier { get; private set; }

    /// <summary>
    /// Permet de remplacer les intercepteurs posés par défaut.
    /// </summary>
    public Func<Type, Action<InterceptionOptions>> InterceptionOptions { get; set; }

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
    /// Spécifie la durée du cache des listes de référence non statiques.
    /// </summary>
    /// <param name="duration">Durée.</param>
    /// <returns>Config.</returns>
    public ServicesConfig WithReferenceListCacheDuration(TimeSpan duration)
    {
        ReferenceListCacheDuration = duration;
        return this;
    }

    /// <summary>
    /// Spécifie une implémentation de notifier de flush de liste de référence.
    /// </summary>
    /// <returns>Config.</returns>
    public ServicesConfig WithReferenceNotifier<T>()
        where T : IReferenceNotifier
    {
        ReferenceNotifier = typeof(T);
        return this;
    }

    /// <summary>
    /// Spécifie la durée du cache des listes statiques.
    /// </summary>
    /// <param name="duration">Durée.</param>
    /// <returns>Config.</returns>
    public ServicesConfig WithStaticListCacheDuration(TimeSpan duration)
    {
        StaticListCacheDuration = duration;
        return this;
    }
}
