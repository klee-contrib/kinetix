using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Resources;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kinetix.Database
{
    /// <summary>
    /// Manager pour la gestion des appels base de données.
    /// </summary>
    public sealed class SqlServerManager
    {
        private const string SqlServerTransactionalContext = "SqlServerTransactionalContext";

        private readonly TransactionalContext _context;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _connectionSettings = new Dictionary<string, string>();
        private readonly List<ResourceManager> _constraintMessagesResources = new List<ResourceManager>();
        private readonly List<ResourceManager> _includeQueryResources = new List<ResourceManager>();
        private readonly Dictionary<string, object> _constValues = new Dictionary<string, object>();

        private readonly ILogger<SqlServerManager> _logger;
        private readonly IHttpContextAccessor _httpContext;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public SqlServerManager(ILogger<SqlServerManager> logger, TransactionalContext context, IConfiguration configuration, IHttpContextAccessor httpContext = null)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _httpContext = httpContext;
        }

        /// <summary>
        /// Retourne le context transactionnel courant.
        /// </summary>
        /// <returns>Context transactionnel.</returns>
        public TransactionalContext CurrentTransactionalContext => _context;

        /// <summary>
        /// Manager de resources.
        /// </summary>
        /// <param name="manager">Manager.</param>
        public void RegisterConstraintMessageResource(ResourceManager manager)
        {
            _constraintMessagesResources.Add(manager);
        }

        /// <summary>
        /// Manager de resources d'include pour les queries.
        /// </summary>
        /// <param name="manager">Manager.</param>
        public void RegisterIncludeQueryResource(ResourceManager manager)
        {
            _includeQueryResources.Add(manager);
        }

        /// <summary>
        /// Enregistre les valeurs de constantes statiques des DTOs pour une assembly donnée.
        /// </summary>
        /// <param name="assembly">L'assembly à analyser.</param>
        public void RegisterConstDataTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsPublic && type.IsClass && type.Namespace != null && type.Namespace.IndexOf("DataContract", StringComparison.Ordinal) != -1)
                {
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        var isConstant = field.FieldType.IsPrimitive || field.FieldType == typeof(string);
                        if (isConstant)
                        {
                            _constValues[type.Name + "." + field.Name] = field.GetRawConstantValue();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne la valeur de la constante statique a partir de son nom court (ClassName.FieldName).
        /// </summary>
        /// <param name="shortName">Nom court.</param>
        /// <returns>Valeur de la constante statique.</returns>
        public object GetConstValueByShortName(string shortName)
        {
            if (string.IsNullOrEmpty(shortName))
            {
                throw new ArgumentNullException("shortName");
            }

            return _constValues[shortName];
        }

        /// <summary>
        /// Fournit une connexion base de données.
        /// Si aucune connexion n'est associée au context, une nouvelle connexion
        /// est ouverte.
        /// Une transaction doit être disponible.
        /// La connexion est enregistrée dans ce context et sera automatiquement libérée.
        /// </summary>
        /// <param name="connectionName">Nom de la source de données.</param>
        /// <returns>Connexion.</returns>
        public SqlServerConnection ObtainConnection(string connectionName)
        {
            if (_httpContext != null && Transaction.Current == null)
            {
                throw new NotSupportedException("Pas de context transactionnel !");
            }

            var connection = _context.GetConnection(connectionName);

            if (connection == null)
            {
                connection = new SqlServerConnection(_configuration.GetConnectionString(connectionName), connectionName);
                _context.RegisterConnection(connection);
                connection.Open();
            }

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            return connection;
        }

        /// <summary>
        /// Retourne le message d'erreur associée à une violation de contrainte.
        /// </summary>
        /// <param name="indexName">Nom de l'index.</param>
        /// <param name="violation">Type de violation.</param>
        /// <returns>Message d'erreur ou null.</returns>
        public string GetConstraintMessage(string indexName, SqlServerConstraintViolation violation)
        {
            string resourceName = indexName;
            if (violation == SqlServerConstraintViolation.ForeignKey)
            {
                resourceName += "_missing";
            }

            foreach (ResourceManager manager in _constraintMessagesResources)
            {
                try
                {
                    string constraintMessage = manager.GetString(resourceName);
                    if (!string.IsNullOrEmpty(constraintMessage))
                    {
                        return constraintMessage;
                    }
                }
                catch (MissingManifestResourceException)
                {
                    continue;
                }
            }

            _logger?.LogWarning("La ressource " + resourceName + " n'a pas été trouvée, utilisation du message par défault.");

            switch (violation)
            {
                case SqlServerConstraintViolation.ForeignKey:
                    resourceName = "FK_MISSING_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.ReferenceKey:
                    resourceName = "FK_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.Unique:
                    resourceName = "UK_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.Check:
                    resourceName = "CK_DEFAULT_MESSAGE";
                    break;
                default:
                    return null;
            }

            foreach (ResourceManager manager in _constraintMessagesResources)
            {
                try
                {
                    return manager.GetString(resourceName);
                }
                catch (MissingManifestResourceException)
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// Cherche une query dans les ressources enregistrée.
        /// </summary>
        /// <param name="resourceName">Nom de la ressource.</param>
        /// <returns>La query à inclure.</returns>
        public string GetIncludeQuery(string resourceName)
        {
            foreach (ResourceManager manager in _includeQueryResources)
            {
                try
                {
                    string includeQuery = manager.GetString(resourceName);
                    if (!string.IsNullOrEmpty(includeQuery))
                    {
                        return includeQuery;
                    }
                }
                catch (MissingManifestResourceException)
                {
                    continue;
                }
            }

            _logger?.LogWarning("La ressource " + resourceName + " n'a pas été trouvée pour l'inclusion de requètes.");

            return null;
        }
    }
}
