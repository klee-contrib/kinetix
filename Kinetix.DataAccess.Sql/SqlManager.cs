using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Manager pour la gestion des appels base de données.
    /// </summary>
    public sealed class SqlManager
    {
        private readonly Dictionary<string, object> _constValues = new();
        private readonly List<ResourceManager> _constraintMessagesResources = new();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="config">Config.</param>
        public SqlManager(SqlConfig config)
        {
            if (!config.ConnectionStrings.TryGetValue(SqlConfig.DefaultConnection, out var _))
            {
                throw new ArgumentException("Une source de données par défaut doit être renseignée.");
            }

            foreach (var type in config.ConstDataTypes.SelectMany(a => a.GetTypes()))
            {
                if (type.IsPublic && type.IsClass && type.Namespace != null && type.Namespace.IndexOf("DataContract", StringComparison.Ordinal) != -1)
                {
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        _constValues[type.Name + "." + field.Name] = field.GetRawConstantValue();
                    }
                }
            }

            _constraintMessagesResources.AddRange(config.ResourceManagers);
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
        /// Retourne le message d'erreur associée à une violation de contrainte.
        /// </summary>
        /// <param name="indexName">Nom de l'index.</param>
        /// <param name="violation">Type de violation.</param>
        /// <returns>Message d'erreur ou null.</returns>
        internal string GetConstraintMessage(string indexName, SqlConstraintViolation violation)
        {
            var resourceName = indexName;
            if (violation == SqlConstraintViolation.ForeignKey)
            {
                resourceName += "_missing";
            }

            foreach (var manager in _constraintMessagesResources)
            {
                try
                {
                    var constraintMessage = manager.GetString(resourceName);
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

            switch (violation)
            {
                case SqlConstraintViolation.ForeignKey:
                    resourceName = "FK_MISSING_DEFAULT_MESSAGE";
                    break;
                case SqlConstraintViolation.ReferenceKey:
                    resourceName = "FK_DEFAULT_MESSAGE";
                    break;
                case SqlConstraintViolation.Unique:
                    resourceName = "UK_DEFAULT_MESSAGE";
                    break;
                case SqlConstraintViolation.Check:
                    resourceName = "CK_DEFAULT_MESSAGE";
                    break;
                default:
                    return null;
            }

            foreach (var manager in _constraintMessagesResources)
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
    }
}
