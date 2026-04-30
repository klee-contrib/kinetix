using System.Collections;
using System.Globalization;
using System.Reflection;
using Kinetix.Modeling;
using Kinetix.Modeling.Exceptions;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services;

/// <summary>
/// Gestionnaire des données de références.
/// </summary>
/// <param name="provider">Service provider.</param>
/// <param name="cacheDuration">Durée du cache des listes de références.</param>
public class ReferenceManager(IServiceProvider provider, TimeSpan cacheDuration) : IReferenceManager
{
    private readonly HybridCache _cache = provider.GetRequiredService<HybridCache>();
    private readonly bool _hasDistributedCache =
        provider.GetService<IDistributedCache>() is not null and not MemoryDistributedCache;
    private readonly IDictionary<Type, ReferenceAccessor> _referenceAccessors =
        new Dictionary<Type, ReferenceAccessor>();
    private readonly IReferenceNotifier? _referenceNotifier = provider.GetService<IReferenceNotifier>();
    private readonly IMemoryCache _syncCache = provider.GetRequiredService<IMemoryCache>();

    /// <inheritdoc />
    public IEnumerable<string> ReferenceLists =>
        _referenceAccessors.Values.Select(accessor => accessor.ReferenceType.Name).Order();

    /// <inheritdoc cref="IReferenceManager.CheckReferenceKeysAsync" />
    public async Task CheckReferenceKeysAsync(object? bean, CancellationToken ct = default)
    {
        var errors = await CheckReferenceKeysInternal(bean, ct);
        if (errors.Any())
        {
            throw new BusinessException(errors);
        }
    }

    /// <inheritdoc cref="IReferenceManager.FlushCache{T}()" />
    public void FlushCache<T>()
        where T : notnull
    {
        FlushCache(typeof(T).Name);
    }

    /// <inheritdoc cref="IReferenceManager.FlushCache(string)" />
    public void FlushCache(string referenceName)
    {
        var key = GetCacheKey(referenceName);
        _syncCache.Remove(key);
        if (_referenceNotifier is ISyncReferenceNotifier syncNotifier)
        {
            syncNotifier.NotifyFlush(referenceName);
        }
    }

    /// <inheritdoc cref="IReferenceManager.FlushCacheAsync{T}(CancellationToken)" />
    public Task FlushCacheAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        return FlushCacheAsync(typeof(T).Name, ct);
    }

    /// <inheritdoc cref="IReferenceManager.FlushCacheAsync(string, CancellationToken)" />
    public async Task FlushCacheAsync(string referenceName, CancellationToken ct = default)
    {
        var key = GetCacheKey(referenceName);
        await _cache.RemoveAsync(key, ct);
        if (_referenceNotifier is IAsyncReferenceNotifier asyncNotifier)
        {
            await asyncNotifier.NotifyFlushAsync(referenceName, ct);
        }
        else if (_referenceNotifier is ISyncReferenceNotifier syncNotifier)
        {
            syncNotifier.NotifyFlush(referenceName);
        }
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}()" />
    public ICollection<T> GetReferenceList<T>()
        where T : notnull
    {
        return GetReferenceEntry<T>().Map.Values;
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(Func{T, bool})" />
    public ICollection<T> GetReferenceList<T>(Func<T, bool> predicate)
        where T : notnull
    {
        return GetReferenceList<T>().Where(predicate).ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList(string)" />
    public ICollection<object> GetReferenceList(string referenceName)
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(nameof(GetReferenceList), 1, []);
        return Enumerable.Cast<object>((ICollection)genericMethod!.MakeGenericMethod(type).Invoke(this, [])!).ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceListAsync{T}(CancellationToken)" />
    public async Task<ICollection<T>> GetReferenceListAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        return (await GetReferenceEntryAsync<T>(ct)).Map.Values;
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceListAsync{T}(Func{T, bool}, CancellationToken)" />
    public async Task<ICollection<T>> GetReferenceListAsync<T>(Func<T, bool> predicate, CancellationToken ct = default)
        where T : notnull
    {
        return (await GetReferenceListAsync<T>(ct)).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceListAsync(string, CancellationToken)" />
    public async Task<ICollection<object>> GetReferenceListAsync(string referenceName, CancellationToken ct = default)
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(
            nameof(GetReferenceListAsync),
            1,
            [typeof(CancellationToken)]
        );
        return (await genericMethod!.MakeGenericMethod(type).InvokeAsync<ICollection>(this, [ct]))
            .Cast<object>()
            .ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceMap{T}()" />
    public IDictionary<object, T> GetReferenceMap<T>()
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return GetReferenceList<T>().ToDictionary(x => def.PrimaryKey.GetValue(x)!, x => x);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceMap{T}(Func{T, bool})" />
    public IDictionary<object, T> GetReferenceMap<T>(Func<T, bool> predicate)
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return GetReferenceList(predicate).ToDictionary(x => def.PrimaryKey.GetValue(x)!, x => x);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceMapAsync{T}(CancellationToken)" />
    public async Task<IDictionary<object, T>> GetReferenceMapAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return (await GetReferenceListAsync<T>(ct)).ToDictionary(x => def.PrimaryKey.GetValue(x)!, x => x);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceMapAsync{T}(Func{T, bool}, CancellationToken)" />
    public async Task<IDictionary<object, T>> GetReferenceMapAsync<T>(
        Func<T, bool> predicate,
        CancellationToken ct = default
    )
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return (await GetReferenceListAsync(predicate, ct)).ToDictionary(x => def.PrimaryKey.GetValue(x)!, x => x);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObject{T}(object?)" />
    public T? GetReferenceObject<T>(object? primaryKey)
        where T : notnull
    {
        if (primaryKey == null)
        {
            return default;
        }

        return GetReferenceEntry<T>().GetReferenceObject(primaryKey);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObject{T}(Func{T, bool})" />
    public T? GetReferenceObject<T>(Func<T, bool> predicate)
        where T : notnull
    {
        return GetReferenceEntry<T>().GetReferenceObject(predicate);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObject(string, object?)" />
    public object? GetReferenceObject(string referenceName, object? primaryKey)
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(nameof(GetReferenceObject), 1, [typeof(object)]);
        return genericMethod!.MakeGenericMethod(type).Invoke(this, [primaryKey]);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObjectAsync{T}(object?, CancellationToken)" />
    public async Task<T?> GetReferenceObjectAsync<T>(object? primaryKey, CancellationToken ct = default)
        where T : notnull
    {
        if (primaryKey == null)
        {
            return default;
        }

        return (await GetReferenceEntryAsync<T>(ct)).GetReferenceObject(primaryKey);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObjectAsync{T}(Func{T, bool}, CancellationToken)" />
    public async Task<T?> GetReferenceObjectAsync<T>(Func<T, bool> predicate, CancellationToken ct = default)
        where T : notnull
    {
        return (await GetReferenceEntryAsync<T>(ct)).GetReferenceObject(predicate);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObjectAsync(string, object?, CancellationToken)" />
    public async Task<object?> GetReferenceObjectAsync(
        string referenceName,
        object? primaryKey,
        CancellationToken ct = default
    )
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(
            nameof(GetReferenceObjectAsync),
            1,
            [typeof(object), typeof(CancellationToken)]
        );
        return await genericMethod!.MakeGenericMethod(type).InvokeAsync<object>(this, [primaryKey, ct]);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue{T}(object?)" />
    public string? GetReferenceValue<T>(object? primaryKey)
        where T : notnull
    {
        return primaryKey == null ? null : GetReferenceValue(GetReferenceObject<T>(primaryKey));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue{T}(Func{T, bool})" />
    public string? GetReferenceValue<T>(Func<T, bool> predicate)
        where T : notnull
    {
        return GetReferenceValue(GetReferenceObject(predicate));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue(string, object?)" />
    public string? GetReferenceValue(string referenceName, object? primaryKey)
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(nameof(GetReferenceValue), 1, [typeof(object)]);
        return (string?)genericMethod!.MakeGenericMethod(type).Invoke(this, [primaryKey]);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueAsync{T}(object?, CancellationToken)" />
    public async Task<string?> GetReferenceValueAsync<T>(object? primaryKey, CancellationToken ct = default)
        where T : notnull
    {
        return primaryKey == null ? null : GetReferenceValue(await GetReferenceObjectAsync<T>(primaryKey, ct));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueAsync{T}(Func{T, bool}, CancellationToken)" />
    public async Task<string?> GetReferenceValueAsync<T>(Func<T, bool> predicate, CancellationToken ct = default)
        where T : notnull
    {
        return GetReferenceValue(await GetReferenceObjectAsync(predicate, ct));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueAsync(string, object?, CancellationToken)" />
    public async Task<string?> GetReferenceValueAsync(
        string referenceName,
        object? primaryKey,
        CancellationToken ct = default
    )
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(
            nameof(GetReferenceValueAsync),
            1,
            [typeof(object), typeof(CancellationToken)]
        );
        return await genericMethod!.MakeGenericMethod(type).InvokeAsync<string>(this, [primaryKey, ct]);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMap{T}()" />
    public IDictionary<object, string> GetReferenceValueMap<T>()
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return GetReferenceList<T>().ToDictionary(x => def.PrimaryKey.GetValue(x)!, GetRequiredReferenceValue);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMap{T}(Func{T, bool})" />
    public IDictionary<object, string> GetReferenceValueMap<T>(Func<T, bool> predicate)
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return GetReferenceList(predicate).ToDictionary(x => def.PrimaryKey.GetValue(x)!, GetRequiredReferenceValue);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMap(string)" />
    public IDictionary<object, string> GetReferenceValueMap(string referenceName)
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(nameof(GetReferenceValueMap), 1, []);
        return (IDictionary<object, string>)genericMethod!.MakeGenericMethod(type).Invoke(this, [])!;
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMapAsync{T}(CancellationToken)" />
    public async Task<IDictionary<object, string>> GetReferenceValueMapAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return (await GetReferenceListAsync<T>(ct)).ToDictionary(
            x => def.PrimaryKey.GetValue(x)!,
            GetRequiredReferenceValue
        );
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMapAsync{T}(Func{T, bool}, CancellationToken)" />
    public async Task<IDictionary<object, string>> GetReferenceValueMapAsync<T>(
        Func<T, bool> predicate,
        CancellationToken ct = default
    )
        where T : notnull
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return (await GetReferenceListAsync(predicate, ct)).ToDictionary(
            x => def.PrimaryKey.GetValue(x)!,
            GetRequiredReferenceValue
        );
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValueMapAsync(string, CancellationToken)" />
    public async Task<IDictionary<object, string>> GetReferenceValueMapAsync(
        string referenceName,
        CancellationToken ct = default
    )
    {
        var type = GetTypeFromName(referenceName);
        var genericMethod = typeof(ReferenceManager).GetMethod(
            nameof(GetReferenceValueMapAsync),
            1,
            [typeof(CancellationToken)]
        );
        return await genericMethod!.MakeGenericMethod(type).InvokeAsync<IDictionary<object, string>>(this, [ct]);
    }

    /// <summary>
    /// Enregistre les accesseurs de listes de référence une interface.
    /// </summary>
    /// <param name="contractType">Type du contrat d'interface.</param>
    internal void RegisterAccessors(Type contractType)
    {
        foreach (var method in contractType.GetMethods())
        {
            var returnType = method.ReturnType;

            var attribute = method.GetCustomAttribute<ReferenceAccessorAttribute>();
            if (attribute != null)
            {
                var isAsync = returnType.GetGenericTypeDefinition() == typeof(Task<>);

                if (
                    !(
                        returnType.IsGenericType
                        && (
                            returnType.GetGenericTypeDefinition() == typeof(ICollection<>)
                            || returnType.GetGenericTypeDefinition() == typeof(Task<>)
                                && returnType.GetGenericArguments()[0].IsGenericType
                                && returnType.GetGenericArguments()[0].GetGenericTypeDefinition()
                                    == typeof(ICollection<>)
                        )
                    )
                )
                {
                    throw new NotSupportedException(
                        $"L'accesseur {method.Name} doit retourner une ICollection<T> ou une Task<ICollection<T>>"
                    );
                }

                if (
                    isAsync
                    && (
                        method.GetParameters().Length != 1
                        || method.GetParameters()[0].ParameterType != typeof(CancellationToken)
                    )
                )
                {
                    throw new NotSupportedException(
                        $"L'accesseur {method.Name} ne doit prendre qu'un CancellationToken en paramètre."
                    );
                }
                else if (!isAsync && method.GetParameters().Length != 0)
                {
                    throw new NotSupportedException($"L'accesseur {method.Name} ne doit prendre aucun paramètre.");
                }

                var accessor = new ReferenceAccessor
                {
                    ContractType = contractType,
                    Method = method,
                    IsAsync = isAsync,
                    ReferenceType = isAsync
                        ? returnType.GetGenericArguments()[0].GetGenericArguments()[0]
                        : returnType.GetGenericArguments()[0],
                };

                if (_referenceAccessors.ContainsKey(accessor.ReferenceType))
                {
                    throw new NotSupportedException();
                }

                _referenceAccessors.Add(accessor.ReferenceType, accessor);
            }
        }
    }

    private static string GetCacheKey(string referenceName)
    {
        return $"ReferenceManager_{CultureInfo.CurrentCulture.Name}_{referenceName}";
    }

    private static string? GetReferenceValue<T>(T reference)
    {
        if (reference is null)
        {
            return null;
        }

        var definition = BeanDescriptor.GetDefinition(reference);
        return definition.DefaultProperty?.GetValue(reference)?.ToString();
    }

    private static string GetRequiredReferenceValue<T>(T reference)
        where T : notnull
    {
        var definition = BeanDescriptor.GetDefinition(reference);
        return definition.DefaultProperty?.GetValue(reference)?.ToString()!;
    }

    private async Task<ErrorMessageCollection> CheckReferenceKeysInternal(object? bean, CancellationToken ct = default)
    {
        var errors = new ErrorMessageCollection();

        if (bean is null || bean is string || bean.GetType().IsValueType)
        {
            return errors;
        }

        if (bean is IEnumerable list)
        {
            foreach (var item in list)
            {
                foreach (var error in await CheckReferenceKeysInternal(item, ct))
                {
                    errors.AddEntry(error);
                }
            }
        }
        else
        {
            var descriptor = BeanDescriptor.GetDefinition(bean.GetType());
            if (descriptor != null)
            {
                foreach (var property in descriptor.Properties)
                {
                    var value = property.GetValue(bean);
                    if (value != null)
                    {
                        if (property.ReferenceType != null)
                        {
                            var refDescriptor = BeanDescriptor.GetDefinition(property.ReferenceType);
                            if (refDescriptor.IsReference)
                            {
                                var keys = (await GetReferenceValueMapAsync(refDescriptor.BeanType.Name, ct)).Keys;

                                if (!keys.Contains(value))
                                {
                                    errors.AddEntry(
                                        new ErrorMessage(
                                            $"La valeur '{value}' n'est pas valide pour la propriété '{property.PropertyName}'. Valeurs attendues : {string.Join(", ", keys.Select(k => $"'{k}'"))}."
                                        )
                                    );
                                }
                            }
                        }
                        else
                        {
                            foreach (var error in await CheckReferenceKeysInternal(value, ct))
                            {
                                errors.AddEntry(error);
                            }
                        }
                    }
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Construit l'entrée du cache synchrone associé à la référence demandée.
    /// </summary>
    /// <returns>L'entrée de cache.</returns>
    private ReferenceEntry<T> GetReferenceEntry<T>()
        where T : notnull
    {
        var key = GetCacheKey(typeof(T).Name);
        return new ReferenceEntry<T>
        {
            Map = _syncCache.GetOrCreate(
                key,
                memOpt =>
                {
                    memOpt.AbsoluteExpirationRelativeToNow = cacheDuration;

                    if (_referenceNotifier is ISyncReferenceNotifier syncNotifier)
                    {
                        syncNotifier.RegisterFlush(typeof(T).Name, () => _syncCache.Remove(key));
                    }

                    var def = BeanDescriptor.GetDefinition(GetTypeFromName(typeof(T).Name));
                    return InvokeReferenceAccessor<T>()
                        .ToDictionary(r => def.PrimaryKey.GetValue(r)!.ToString()!, r => r);
                }
            )!,
        };
    }

    /// <summary>
    /// Construit l'entrée du cache associé à la référence demandée.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>L'entrée de cache.</returns>
    private async Task<ReferenceEntry<T>> GetReferenceEntryAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        var key = GetCacheKey(typeof(T).Name);
        return new ReferenceEntry<T>
        {
            Map = await _cache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    async Task Flusher() =>
                        await _cache.SetAsync(
                            key,
                            await _cache.GetOrCreateAsync<IDictionary<string, T>>(
                                key,
                                async ct => new Dictionary<string, T>(),
                                new()
                                {
                                    Flags =
                                        HybridCacheEntryFlags.DisableLocalCache
                                        | HybridCacheEntryFlags.DisableDistributedCacheWrite,
                                },
                                cancellationToken: ct
                            ),
                            new()
                            {
                                LocalCacheExpiration = TimeSpan.Zero,
                                Expiration = _hasDistributedCache ? cacheDuration : TimeSpan.Zero,
                            },
                            cancellationToken: ct
                        );

                    if (_referenceNotifier is IAsyncReferenceNotifier asyncNotifier)
                    {
                        await asyncNotifier.RegisterFlushAsync(typeof(T).Name, Flusher, ct);
                    }
                    else if (_referenceNotifier is ISyncReferenceNotifier syncNotifier)
                    {
#pragma warning disable CS4014
                        syncNotifier.RegisterFlush(typeof(T).Name, () => Flusher());
#pragma warning restore CS4014
                    }

                    var def = BeanDescriptor.GetDefinition(typeof(T));
                    return (await InvokeReferenceAccessorAsync<T>(ct)).ToDictionary(
                        r => def.PrimaryKey.GetValue(r)!.ToString()!,
                        r => r
                    );
                },
                new()
                {
                    Expiration = cacheDuration,
                    LocalCacheExpiration = _hasDistributedCache ? TimeSpan.FromMinutes(1) : cacheDuration,
                },
                cancellationToken: ct
            ),
        };
    }

    private Type GetTypeFromName(string referenceName)
    {
        return _referenceAccessors.Values.Single(r => r.ReferenceType.Name == referenceName).ReferenceType;
    }

    /// <summary>
    /// Récupère la liste de référence associée à la référence demandée, via son accesseur synchrone.
    /// </summary>
    /// <returns>La liste de référence.</returns>
    private List<T> InvokeReferenceAccessor<T>()
        where T : notnull
    {
        if (!_referenceAccessors.TryGetValue(typeof(T), out var accessor) || accessor.IsAsync)
        {
            throw new ArgumentException(
                $"Pas d'accesseur synchrone disponible pour la liste {typeof(T).Name}",
                typeof(T).Name
            );
        }

        var service = provider.GetRequiredService(accessor.ContractType);

        return Enumerable.Cast<T>((ICollection)accessor.Method.Invoke(service, [])!).ToList();
    }

    /// <summary>
    /// Récupère la liste de référence associée à la référence demandée, via son accesseur.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>La liste de référence.</returns>
    private async Task<ICollection<T>> InvokeReferenceAccessorAsync<T>(CancellationToken ct = default)
        where T : notnull
    {
        if (!_referenceAccessors.TryGetValue(typeof(T), out var accessor))
        {
            throw new ArgumentException($"Pas d'accesseur disponible pour la liste {typeof(T).Name}", typeof(T).Name);
        }

        var service = provider.GetRequiredService(accessor.ContractType);

        return accessor.IsAsync
            ? await accessor.Method.InvokeAsync<ICollection<T>>(service, [ct])
            : Enumerable.Cast<T>((ICollection)accessor.Method.Invoke(service, [])!).ToList();
    }
}
