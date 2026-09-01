using System.Collections;
using Kinetix.Modeling;

namespace Kinetix.Services;

public partial class ReferenceManager
{
    /// <inheritdoc cref="IReferenceManager.FlushCache{T}()" />
    public void FlushCache<T>()
        where T : notnull
    {
        FlushCache(typeof(T).Name);
    }

    /// <inheritdoc cref="IReferenceManager.FlushCache(string)" />
    public void FlushCache(string referenceName)
    {
        FlushCacheAsync(referenceName, CancellationToken.None).Wait(CancellationToken.None);
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

    private ReferenceEntry<T> GetReferenceEntry<T>()
        where T : notnull
    {
        return GetReferenceEntryAsync<T>(default).GetAwaiter().GetResult();
    }
}
