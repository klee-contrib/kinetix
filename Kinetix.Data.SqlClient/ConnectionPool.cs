using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Kinetix.Monitoring;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.Data.SqlClient
{
    public class ConnectionPool
    {
        private readonly AnalyticsManager _analytics;
        private readonly Dictionary<string, string> _connectionSettings;
        private readonly CommandParser _commandParser;
        private readonly int _defaultCommandTimeout;
        private readonly ILogger<SqlServerCommand> _logger;
        private readonly TransactionScopeManager _transactionScopeManager;

        public ConnectionPool(TransactionScopeManager transactionScopeManager, AnalyticsManager analytics, CommandParser commandParser, ILogger<SqlServerCommand> logger, SqlServerConfig config)
        {
            _analytics = analytics;
            _commandParser = commandParser;
            _connectionSettings = config.ConnectionStrings;
            _defaultCommandTimeout = config.DefaultCommandTimeout;
            _logger = logger;
            _transactionScopeManager = transactionScopeManager;
        }

        public SqlServerCommand GetSqlServerCommand(string connectionName, Assembly assembly, string resourcePath)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, assembly, resourcePath) { CommandTimeout = _defaultCommandTimeout };
        }

        public SqlServerCommand GetSqlServerCommand(string connectionName, string procName)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, procName) { CommandTimeout = _defaultCommandTimeout };
        }

        public SqlServerCommand GetSqlServerCommand(string connectionName, string commandName, string commandText)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandText) { CommandTimeout = _defaultCommandTimeout };
        }

        public SqlServerCommand GetSqlServerCommand(string connectionName, string commandName, CommandType commandType)
        {
            return new SqlServerCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandType) { CommandTimeout = _defaultCommandTimeout };
        }

        private SqlServerConnection GetConnection(string datasourceName)
        {
            var transactionContext = _transactionScopeManager.ActiveScope?.GetContext<SqlTransactionContext>();

            if (transactionContext == null)
            {
                throw new InvalidOperationException("Impossible de récupérer une connection en dehors d'un scope de transaction.");
            }

            var connection = transactionContext.Connections.SingleOrDefault(c => c.DataSourceName == datasourceName);

            if (connection == null)
            {
                connection = new SqlServerConnection(datasourceName, _connectionSettings[datasourceName]);
                transactionContext.Connections.Add(connection);
                connection.Open();
            }

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            return connection;
        }
    }
}
