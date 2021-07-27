using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Kinetix.Data.SqlClient
{
    public class SqlServerConfig
    {
        /// <summary>
        /// Nom de la connexion par défaut.
        /// </summary>
        public const string DefaultConnection = "default";

        public Dictionary<string, string> ConnectionStrings { get; } = new Dictionary<string, string>();
        internal List<Assembly> ConstDataTypes { get; } = new List<Assembly>();
        internal int DefaultCommandTimeout { get; private set; } = 30;
        internal List<ResourceManager> ResourceManagers { get; } = new List<ResourceManager>();

        /// <summary>
        /// Enregistre une connection.
        /// </summary>
        /// <param name="name">Nom de la connexion.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Config.</returns>
        public SqlServerConfig AddConnectionString(string name, string connectionString)
        {
            ConnectionStrings.Add(name, connectionString);
            return this;
        }

        /// <summary>
        /// Enregistre les assemblies donnés pour résoudre les constantes.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns>Config.</returns>
        public SqlServerConfig AddConstDataTypes(params Assembly[] assemblies)
        {
            ConstDataTypes.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// Enregistre la connection par défaut.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Config.</returns>
        public SqlServerConfig AddDefaultConnectionString(string connectionString)
        {
            ConnectionStrings.Add(DefaultConnection, connectionString);
            return this;
        }

        /// <summary>
        /// Ajoute les Resx donnés pour résoudre les erreurs de contraintes SQL.
        /// </summary>
        /// <param name="resourceManagers">ResourceManagers.</param>
        /// <returns>Config.</returns>
        public SqlServerConfig AddConstraintMessages(params ResourceManager[] resourceManagers)
        {
            ResourceManagers.AddRange(resourceManagers);
            return this;
        }

        /// <summary>
        /// Configure le timeout par défaut des commandes SQL.
        /// </summary>
        /// <param name="defaultCommandTimeout">Timeout.</param>
        /// <returns>Config.</returns>
        public SqlServerConfig WithDefaultCommandTimeout(int defaultCommandTimeout)
        {
            DefaultCommandTimeout = defaultCommandTimeout;
            return this;
        }
    }
}
