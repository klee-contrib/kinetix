using System.Collections;
using System.Reflection;
using Kinetix.Modeling;
using Kinetix.Modeling.Annotations;
using Kinetix.Modeling.Exceptions;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services;

/// <summary>
/// Gestionnaire des données de références.
/// </summary>
public class ReferenceManager : IReferenceManager
{
    private const string ReferenceListsCache = "ReferenceLists";
    private const string StaticListsCache = "StaticLists";

    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _provider;

    private readonly TimeSpan _referenceListCacheDuration;
    private readonly TimeSpan _staticListCacheDuration;
    private static readonly object lockObj = new();

    private readonly IDictionary<string, Accessor> _referenceAccessors = new Dictionary<string, Accessor>();

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="provider">Service provider.</param>
    /// <param name="referenceListCacheDuration">Durée du cache des listes de références.</param>
    /// <param name="staticListCacheDuration">Durée du cache des listes statiques.</param>
    public ReferenceManager(IServiceProvider provider, TimeSpan staticListCacheDuration, TimeSpan referenceListCacheDuration)
    {
        _cache = provider.GetService<IMemoryCache>();
        _referenceListCacheDuration = referenceListCacheDuration;
        _staticListCacheDuration = staticListCacheDuration;
        _provider = provider;
    }

    /// <inheritdoc />
    public IEnumerable<string> ReferenceLists => _referenceAccessors.Values.Select(accessor => accessor.Name).OrderBy(x => x);

    /// <inheritdoc cref="IReferenceManager.CheckReferenceKeys" />
    public void CheckReferenceKeys(object bean)
    {
        var errors = CheckReferenceKeysInternal(bean);
        if (errors.Any())
        {
            throw new BusinessException(errors);
        }
    }

    /// <inheritdoc cref="IReferenceManager.FlushCache" />
    public void FlushCache(string referenceName = null)
    {
        if (referenceName == null)
        {
            _cache.Remove(StaticListsCache);
            _cache.Remove(ReferenceListsCache);
        }
        else
        {
            GetCacheByType(GetTypeFromName(referenceName)).Remove(referenceName);
        }
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(string)" />
    public ICollection<object> GetReferenceList(Type type)
    {
        return GetReferenceEntry(type.Name).List;
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(string)" />
    public ICollection<object> GetReferenceList(string referenceName)
    {
        return GetReferenceEntry(referenceName).List;
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(string)" />
    public ICollection<T> GetReferenceList<T>(string referenceName = null)
    {
        return referenceName == null
            ? GetReferenceList(typeof(T)).Cast<T>().ToList()
            : GetReferenceList(referenceName).Cast<T>().ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(Func{T, bool}, string)" />
    public ICollection<T> GetReferenceList<T>(Func<T, bool> predicate, string referenceName = null)
    {
        return GetReferenceList<T>(referenceName)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceListByCriteria" />
    public ICollection<T> GetReferenceList<T>(T criteria)
    {
        var beanPropertyDescriptorList =
            BeanDescriptor.GetDefinition(criteria).Properties
                .Where(property => property.GetValue(criteria) != null);

        return GetReferenceList<T>()
            .Where(bean => beanPropertyDescriptorList.All(property => property.GetValue(criteria).Equals(property.GetValue(bean))))
            .ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceList" />
    public ICollection<T> GetReferenceList<T>(object[] primaryKeys)
    {
        var definition = BeanDescriptor.GetDefinition(typeof(T));
        return GetReferenceList<T>()
            .Where(bean => primaryKeys.Contains(definition.PrimaryKey.GetValue(bean)))
            .ToList();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObject(object)" />
    public T GetReferenceObject<T>(object primaryKey)
    {
        return GetReferenceEntry(typeof(T).Name).GetReferenceObject<T>(primaryKey);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceObject(Func{T, bool}, string)" />
    public T GetReferenceObject<T>(Func<T, bool> predicate)
    {
        return GetReferenceEntry(typeof(T).Name).GetReferenceObject(predicate);
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue{T}(object)" />
    public string GetReferenceValue<T>(object primaryKey)
    {
        return primaryKey == null
            ? null
            : GetReferenceValue(GetReferenceObject<T>(primaryKey));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue{T}(Func{T, bool})" />
    public string GetReferenceValue<T>(Func<T, bool> predicate)
    {
        return GetReferenceValue(GetReferenceObject(predicate));
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue{T}(T) />
    public string GetReferenceValue<T>(T reference)
    {
        var definition = BeanDescriptor.GetDefinition(reference);
        return definition.DefaultProperty.GetValue(reference).ToString();
    }

    /// <inheritdoc cref="IReferenceManager.GetReferenceValue(Type, object) />
    public string GetReferenceValue(Type type, object primaryKey)
    {
        return primaryKey == null
            ? null
            : GetReferenceValue(GetReferenceEntry(type.Name).GetReferenceObject(primaryKey));
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
                if (!returnType.IsGenericType ||
                    !typeof(ICollection<>).Equals(returnType.GetGenericTypeDefinition()) &&
                    returnType.GetGenericTypeDefinition().GetInterface(typeof(ICollection<>).Name) == null)
                {
                    throw new NotSupportedException($"L'accesseur {method.Name} doit retourner une ICollection générique.");
                }

                if (method.GetParameters().Length != 0)
                {
                    throw new NotSupportedException($"L'accesseur {method.Name} ne doit pas prendre de paramètres.");
                }

                var accessor = new Accessor
                {
                    ContractType = contractType,
                    Method = method,
                    ReferenceType = returnType.GetGenericArguments()[0],
                    Name = attribute.Name ?? returnType.GetGenericArguments()[0].Name
                };

                if (_referenceAccessors.ContainsKey(accessor.Name))
                {
                    throw new NotSupportedException();
                }

                _referenceAccessors.Add(accessor.Name, accessor);
            }
        }
    }

    private ErrorMessageCollection CheckReferenceKeysInternal(object bean)
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
                foreach (var error in CheckReferenceKeysInternal(item))
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
                                var keyList = GetReferenceList(property.ReferenceType).Select(item => refDescriptor.PrimaryKey.GetValue(item).ToString());
                                if (!keyList.Contains(value.ToString()))
                                {
                                    errors.AddEntry(new ErrorMessage($"La valeur '{value}' n'est pas valide pour la propriété '{property.PropertyName}'. Valeurs attendues : {string.Join(", ", keyList.Select(k => $"'{k}'"))}."));
                                }
                            }
                        }
                        else
                        {
                            foreach (var error in CheckReferenceKeysInternal(value))
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
    /// Récupère le cache associé au type de référence demandé.
    /// </summary>
    /// <param name="type">Le type de référence.</param>
    /// <returns>Le cache.</returns>
    private ReferenceCache GetCacheByType(Type type)
    {
        var attr = type.GetCustomAttribute<ReferenceAttribute>();
        return attr == null
            ? throw new NotSupportedException($"Le type {type} n'est pas une liste de référence.")
            : _cache.GetOrCreate(attr.IsStatic ? StaticListsCache : ReferenceListsCache, cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = attr.IsStatic ? _staticListCacheDuration : _referenceListCacheDuration;
                return new ReferenceCache();
            });
    }


    /// <summary>
    /// Récupère l'entrée du cache associé à la référence demandée.
    /// </summary>
    /// <param name="referenceName">Nom de la liste.</param>
    /// <returns>L'entrée de cache.</returns>
    private ReferenceEntry GetReferenceEntry(string referenceName)
    {
        var type = GetTypeFromName(referenceName);
        var cache = GetCacheByType(type);

        lock (lockObj)
        {
            if (!cache.TryGetValue(referenceName, out var entry))
            {
                entry = BuildReferenceEntry(referenceName);
                cache.Add(referenceName, entry);
            }
            return entry;
        }
    }

    private Type GetTypeFromName(string referenceName)
    {
        return _referenceAccessors.Values.Single(r => r.Name == referenceName).ReferenceType;
    }

    /// <summary>
    /// Construit l'entrée du cache associé à la référence demandée.
    /// </summary>
    /// <param name="referenceName">Nom de la liste.</param>
    /// <returns>L'entrée de cache.</returns>
    private ReferenceEntry BuildReferenceEntry(string referenceName)
    {
        var referenceList = InvokeReferenceAccessor(referenceName);
        return new ReferenceEntry(referenceList, BeanDescriptor.GetDefinition(GetTypeFromName(referenceName)));
    }

    /// <summary>
    /// Récupère la liste de référence associée à la référence demandée, via son accesseur.
    /// </summary>
    /// <param name="referenceName">Nom de la liste.</param>
    /// <returns>La liste de référence.</returns>
    private ICollection<object> InvokeReferenceAccessor(string referenceName)
    {
        if (!_referenceAccessors.ContainsKey(referenceName))
        {
            throw new ArgumentException($"Pas d'accesseur disponible pour la liste {referenceName}");
        }

        var accessor = _referenceAccessors[referenceName];

        var service = _provider.GetService(accessor.ContractType);
        var list = accessor.Method.Invoke(service, null);

        var coll = (ICollection)list;
        return coll == null
            ? throw new ArgumentException(list.GetType().Name)
            : coll.Cast<object>().ToList();
    }
}
