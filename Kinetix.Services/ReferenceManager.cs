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
        private const string ReferenceLists = "ReferenceLists";
        private const string StaticLists = "StaticLists";

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
        public IEnumerable<Type> ReferenceTypes => _referenceAccessors.Values.Select(accessor => accessor.ReferenceType).Distinct();

        /// <inheritdoc cref="IReferenceManager.FlushCache" />
        public void FlushCache(Type type = null, string referenceName = null)
        {
            if (type == null)
            {
                _cache.Remove(StaticLists);
                _cache.Remove(ReferenceLists);
            }
            else
            {
                GetCacheByType(type).Remove(referenceName ?? type.FullName);
            }
        }

        /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(string)" />
        public ICollection<object> GetReferenceList(Type type, string referenceName = null)
        {
            return GetReferenceEntry(type, referenceName).List;
        }

        /// <inheritdoc cref="IReferenceManager.GetReferenceList{T}(string)" />
        public ICollection<T> GetReferenceList<T>(string referenceName = null)
        {
            return GetReferenceList(typeof(T), referenceName).Cast<T>().ToList();
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
                .Where(bean => !beanPropertyDescriptorList
                    .Any(property => property.PrimitiveType != null && property.GetValue(criteria) != property.GetValue(bean)))
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
            return GetReferenceEntry(typeof(T)).GetReferenceObject<T>(primaryKey);
        }

        /// <inheritdoc cref="IReferenceManager.GetReferenceObject(Func{T, bool}, string)" />
        public T GetReferenceObject<T>(Func<T, bool> predicate, string referenceName = null)
        {
            return GetReferenceEntry(typeof(T), referenceName).GetReferenceObject(predicate);
        }

        public string GetReferenceValue<T>(object primaryKey, Expression<Func<T, object>> propertySelector = null)
        {
            return GetReferenceValue(GetReferenceObject<T>(primaryKey), propertySelector);
        }

        public string GetReferenceValue<T>(Func<T, bool> predicate, Expression<Func<T, object>> propertySelector = null, string referenceName = null)
        {
            return GetReferenceValue(GetReferenceObject(predicate, referenceName), propertySelector);
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
                        Name = attribute.Name
                    };

                    var name = accessor.Name ?? accessor.ReferenceType.FullName;
                    if (_referenceAccessors.ContainsKey(name))
                    {
                        throw new NotSupportedException();
                    }

                    _referenceAccessors.Add(name, accessor);
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

            return _cache.GetOrCreate(attr.IsStatic ? StaticLists : ReferenceLists, cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = attr.IsStatic ? _staticListCacheDuration : _referenceListCacheDuration;
                return new ReferenceCache();
            });
        }

        /// <summary>
        /// Récupère l'entrée du cache associé à la référence demandée.
        /// </summary>
        /// <param name="type">Type de référence.</param>
        /// <param name="referenceName">Nom de la liste (si défini).</param>
        /// <returns>L'entrée de cache.</returns>
        private ReferenceEntry GetReferenceEntry(Type type, string referenceName = null)
        {
            var refName = referenceName ?? type.FullName;
            var cache = GetCacheByType(type);

            if (!cache.TryGetValue(refName, out var entry))
            {
                entry = BuildReferenceEntry(type);
                cache.Add(refName, entry);
            }

            return entry;
        }

        /// <summary>
        /// Construit l'entrée du cache associé à la référence demandée.
        /// </summary>
        /// <param name="type">Type de référence.</param>
        /// <param name="referenceName">Nom de la liste (si défini).</param>
        /// <returns>L'entrée de cache.</returns>
        private ReferenceEntry BuildReferenceEntry(Type type, string referenceName = null)
        {
            var referenceList = InvokeReferenceAccessor(type, referenceName);
            return new ReferenceEntry(referenceList, _beanDescriptor.GetDefinition(type));
        }

        /// <summary>
        /// Récupère la liste de référence associée à la référence demandée, via son accesseur.
        /// </summary>
        /// <param name="type">Type de référence.</param>
        /// <param name="referenceName">Nom de la liste (si défini).</param>
        /// <returns>La liste de référence.</returns>
        private ICollection<object> InvokeReferenceAccessor(Type type, string referenceName = null)
        {
            if (!_referenceAccessors.ContainsKey(type.FullName))
            {
                throw new ArgumentException("Pas d'accesseur disponible pour le type " + type.Name);
            }

            var accessor = _referenceAccessors[referenceName ?? type.FullName];

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