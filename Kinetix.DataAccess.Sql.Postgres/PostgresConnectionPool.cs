using System.Data;
using System.Reflection;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Monitoring.Core;
using Kinetix.Services;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Pool de connections SQL Server.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="transactionScopeManager">Composant injecté.</param>
/// <param name="analytics">Composant injecté.</param>
/// <param name="commandParser">Composant injecté.</param>
/// <param name="config">Composant injecté.</param>
/// <param name="logger">Composant injecté.</param>
internal class PostgresConnectionPool(TransactionScopeManager transactionScopeManager, AnalyticsManager analytics, CommandParser commandParser, ILogger<PostgresCommand> logger, SqlConfig config) : ConnectionPool(transactionScopeManager)
{
    private readonly Dictionary<string, string> _connectionSettings = config.ConnectionStrings;
    private readonly int _defaultCommandTimeout = config.DefaultCommandTimeout;

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string procName)
    {
        return new PostgresCommand(GetConnection(connectionName), logger, commandParser, analytics, procName) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath)
    {
        return new PostgresCommand(GetConnection(connectionName), logger, commandParser, analytics, assembly, resourcePath) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText)
    {
        return new PostgresCommand(GetConnection(connectionName), logger, commandParser, analytics, commandName, commandText) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType)
    {
        return new PostgresCommand(GetConnection(connectionName), logger, commandParser, analytics, commandName, commandType) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    protected override IDbConnection GetNewConnection(string datasourceName)
    {
        return new NpgsqlConnection(_connectionSettings[datasourceName]);
    }
}
