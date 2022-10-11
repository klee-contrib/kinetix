using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed;

public static class DistributedCacheExtensions
{
    /// <summary>
    /// Récupère un objet dans le cache.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à récupérer.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <returns>Objet si trouvé, default sinon.</returns>
    public static T Get<T>(this IDistributedCache cache, string key)
    {
        var item = cache.GetString(key);
        if (item == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(item);
    }

    /// <summary>
    /// Récupère un objet dans le cache.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à récupérer.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <returns>Objet si trouvé, default sinon.</returns>
    public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
    {
        var item = await cache.GetStringAsync(key);
        if (item == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(item);
    }

    /// <summary>
    /// Récupère un objet dans le cache, où l'ajoute s'il n'existe pas.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à mettre en cache.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <param name="factory">Factory pour construire et configurer l'entrée de cache.</param>
    /// <returns>Objet du cache/inséré dans le cache.</returns>
    public static T GetOrSet<T>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, T> factory)
    {
        var item = cache.Get<T>(key);
        if (!Equals(item, default(T)))
        {
            return item;
        }

        var options = new DistributedCacheEntryOptions();
        var entry = factory(options);
        cache.Set(key, entry, options);
        return entry;
    }

    /// <summary>
    /// Récupère un objet dans le cache, où l'ajoute s'il n'existe pas.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à mettre en cache.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <param name="factory">Factory pour construire et configurer l'entrée de cache.</param>
    /// <returns>Objet du cache/inséré dans le cache.</returns>
    public static async Task<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, T> factory)
    {
        var item = await cache.GetAsync<T>(key);
        if (!Equals(item, default(T)))
        {
            return item;
        }

        var options = new DistributedCacheEntryOptions();
        var entry = factory(options);
        await cache.SetAsync(key, entry, options);
        return entry;
    }

    /// <summary>
    /// Ajoute un objet dans le cache.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à mettre en cache.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <param name="item">Entrée de cache.</param>
    /// <param name="options">Option de cache.</param>
    public static void Set<T>(this IDistributedCache cache, string key, T item, DistributedCacheEntryOptions options = null)
    {
        cache.SetString(key, JsonSerializer.Serialize(item), options ?? new());
    }

    /// <summary>
    /// Ajoute un objet dans le cache.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à mettre en cache.</typeparam>
    /// <param name="cache">Cache distribué.</param>
    /// <param name="key">Clé.</param>
    /// <param name="item">Entrée de cache.</param>
    /// <param name="options">Option de cache.</param>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T item, DistributedCacheEntryOptions options = null)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(item), options ?? new());
    }
}
