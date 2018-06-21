using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kinetix.ComponentModel;
using Kinetix.ComponentModel.Annotations;
using Kinetix.Services.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services
{
    /// <summary>
    /// Gestionnaire des données de références.
    /// </summary>
    public class ReferenceManager : IReferenceManager
    {
        private const string ReferenceListsCache = "ReferenceLists";
        private const string StaticListsCache = "StaticLists";

        private readonly BeanDescriptor _beanDescriptor;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _provider;
        private readonly ILogger<ReferenceManager> _logger;

        private readonly TimeSpan _referenceListCacheDuration;
        private readonly TimeSpan _staticListCacheDuration;

        private readonly IDictionary<string, Accessor> _referenceAccessors = new Dictionary<string, Accessor>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        /// <param name="referenceListCacheDuration">Durée du cache des listes de références (par défaut : 10 minutes).</param>
        /// <param name="staticListCacheDuration">Durée du cache des listes statiques (par défaut : 1 heure).</param>
        public ReferenceManager(IServiceProvider provider, TimeSpan? staticListCacheDuration = null, TimeSpan? referenceListCacheDuration = null)
        {
            _beanDescriptor = provider.GetService<BeanDescriptor>();
            _cache = provider.GetService<IMemoryCache>();
            _logger = provider.GetService<ILogger<ReferenceManager>>();
            _referenceListCacheDuration = referenceListCacheDuration ?? TimeSpan.FromMinutes(10);
            _staticListCacheDuration = staticListCacheDuration ?? TimeSpan.FromHours(1);
            _provider = provider;
        }

        /// <inheritdoc />
        public IEnumerable<string> ReferenceLists => _referenceAccessors.Values.Select(accessor => accessor.Name).OrderBy(x => x);

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
            if (referenceName == null)
            {
                return GetReferenceList(typeof(T)).Cast<T>().ToList();
            }
            else
            {
                return GetReferenceList(referenceName).Cast<T>().ToList();
            }
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
                _beanDescriptor.GetDefinition(criteria).Properties
                    .Where(property => property.GetValue(criteria) != null);

            return GetReferenceList<T>()
                .Where(bean => beanPropertyDescriptorList.All(property => property.GetValue(criteria).Equals(property.GetValue(bean))))
                .ToList();
        }

        /// <inheritdoc cref="IReferenceManager.GetReferenceList" />
        public ICollection<T> GetReferenceList<T>(object[] primaryKeys)
        {
            var definition = _beanDescriptor.GetDefinition(typeof(T));
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

        public string GetReferenceValue<T>(object primaryKey, Expression<Func<T, object>> propertySelector = null)
        {
            return GetReferenceValue(GetReferenceObject<T>(primaryKey), propertySelector);
        }

        public string GetReferenceValue<T>(Func<T, bool> predicate, Expression<Func<T, object>> propertySelector = null)
        {
            return GetReferenceValue(GetReferenceObject(predicate), propertySelector);
        }

        public string GetReferenceValue<T>(T reference, Expression<Func<T, object>> propertySelector = null)
        {
            var definition = _beanDescriptor.GetDefinition(reference);
            var property = definition.DefaultProperty;

            if (propertySelector?.Body is MemberExpression mb && mb.Member != null)
            {
                property = definition.Properties[mb.Member.Name];
            }

            return property.ConvertToString(property.GetValue(reference));
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
                    if (!returnType.IsGenericType || (
                        !typeof(ICollection<>).Equals(returnType.GetGenericTypeDefinition()) &&
                        returnType.GetGenericTypeDefinition().GetInterface(typeof(ICollection<>).Name) == null))
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

        /// <summary>
        /// Récupère le cache associé au type de référence demandé.
        /// </summary>
        /// <param name="type">Le type de référence.</param>
        /// <returns>Le cache.</returns>
        private ReferenceCache GetCacheByType(Type type)
        {
            var attr = type.GetCustomAttribute<ReferenceAttribute>();
            if (attr == null)
            {
                throw new NotSupportedException($"Le type {type} n'est pas une liste de référence.");
            }

            return _cache.GetOrCreate(attr.IsStatic ? StaticListsCache : ReferenceListsCache, cacheEntry =>
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

            if (!cache.TryGetValue(referenceName, out var entry))
            {
                entry = BuildReferenceEntry(referenceName);
                cache.Add(referenceName, entry);
            }

            return entry;
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
            return new ReferenceEntry(referenceList, _beanDescriptor.GetDefinition(GetTypeFromName(referenceName)));
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
            if (coll == null)
            {
                throw new ArgumentException(list.GetType().Name);
            }

            return coll.Cast<object>().ToList();
        }
    }
}