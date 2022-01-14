using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Kinetix.Monitoring;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer
{
    /// <summary>
    /// Pool de connections SQL Server.
    /// </summary>
    public class SqlServerConnectionPool : ConnectionPool
    {
        private readonly AnalyticsManager _analytics;
        private readonly CommandParser _commandParser;
        private readonly Dictionary<string, string> _connectionSettings;
        private readonly int _defaultCommandTimeout;
        private readonly ILogger<SqlServerCommand> _logger;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="transactionScopeManager">Composant injecté.</param>
        /// <param name="analytics">Composant injecté.</param>
        /// <param name="commandParser">Composant injecté.</param>
        /// <param name="config">Composant injecté.</param>
        /// <param name="logger">Composant injecté.</param>
        public SqlServerConnectionPool(TransactionScopeManager transactionScopeManager, AnalyticsManager analytics, CommandParser commandParser, ILogger<SqlServerCommand> logger, SqlConfig config)
            : base(transactionScopeManager)
        {
            _analytics = analytics;
            _commandParser = commandParser;
            _logger = logger;
            _connectionSettings = config.ConnectionStrings;
            _defaultCommandTimeout = config.DefaultCommandTimeout;
        }

        /// <inheritdoc />
        public override BaseSqlCommand GetSqlCommand(string connectionName, string procName)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, procName) { CommandTimeout = _defaultCommandTimeout };
        }

        /// <inheritdoc />
        public override BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, assembly, resourcePath) { CommandTimeout = _defaultCommandTimeout };
        }

        /// <inheritdoc />
        public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandText) { CommandTimeout = _defaultCommandTimeout };
        }

        /// <inheritdoc />
        public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandType) { CommandTimeout = _defaultCommandTimeout };
        }

        /// <inheritdoc />
        protected override IDbConnection GetNewConnection(string datasourceName)
        {
            return new SqlConnection(_connectionSettings[datasourceName]);
        }
    }
}
