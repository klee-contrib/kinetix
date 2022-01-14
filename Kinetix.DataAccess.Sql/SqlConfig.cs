using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Config SQL.
    /// </summary>
    public class SqlConfig
    {
        /// <summary>
        /// Nom de la connexion par défaut.
        /// </summary>
        public const string DefaultConnection = "default";

        /// <summary>
        /// ConnectionStrings.
        /// </summary>
        public Dictionary<string, string> ConnectionStrings { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Timeout SQL par défaut.
        /// </summary>
        public int DefaultCommandTimeout { get; private set; } = 30;

        /// <summary>
        /// Assemblies avec les constantes.
        /// </summary>
        internal List<Assembly> ConstDataTypes { get; } = new List<Assembly>();

        /// <summary>
        /// Ressources pour les messages d'erreur.
        /// </summary>
        internal List<ResourceManager> ResourceManagers { get; } = new List<ResourceManager>();

        /// <summary>
        /// Enregistre une connection.
        /// </summary>
        /// <param name="name">Nom de la connexion.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Config.</returns>
        public SqlConfig AddConnectionString(string name, string connectionString)
        {
            ConnectionStrings.Add(name, connectionString);
            return this;
        }

        /// <summary>
        /// Enregistre les assemblies donnés pour résoudre les constantes.
        /// </summary>
        /// <param name="assemblies">Assemblies.</param>
        /// <returns>Config.</returns>
        public SqlConfig AddConstDataTypes(params Assembly[] assemblies)
        {
            ConstDataTypes.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// Ajoute les Resx donnés pour résoudre les erreurs de contraintes SQL.
        /// </summary>
        /// <param name="resourceManagers">ResourceManagers.</param>
        /// <returns>Config.</returns>
        public SqlConfig AddConstraintMessages(params ResourceManager[] resourceManagers)
        {
            ResourceManagers.AddRange(resourceManagers);
            return this;
        }

        /// <summary>
        /// Enregistre la connection par défaut.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Config.</returns>
        public SqlConfig AddDefaultConnectionString(string connectionString)
        {
            ConnectionStrings.Add(DefaultConnection, connectionString);
            return this;
        }

        /// <summary>
        /// Configure le timeout par défaut des commandes SQL.
        /// </summary>
        /// <param name="defaultCommandTimeout">Timeout.</param>
        /// <returns>Config.</returns>
        public SqlConfig WithDefaultCommandTimeout(int defaultCommandTimeout)
        {
            DefaultCommandTimeout = defaultCommandTimeout;
            return this;
        }
    }
}
