using System.Data;
using System.Data.Common;
using System.Reflection;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Monitoring.Core;
using Kinetix.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer;

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
internal class SqlServerConnectionPool(
    TransactionScopeManager transactionScopeManager,
    AnalyticsManager analytics,
    CommandParser commandParser,
    ILogger<SqlServerCommand> logger,
    SqlConfig config
) : ConnectionPool(transactionScopeManager)
{
    private readonly IDictionary<string, string> _connectionSettings = config.ConnectionStrings;
    private readonly int _defaultCommandTimeout = config.DefaultCommandTimeout;

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string procName)
    {
        return new SqlServerCommand(GetConnection(connectionName), logger, commandParser, analytics, procName)
        {
            CommandTimeout = _defaultCommandTimeout,
        };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath)
    {
        return new SqlServerCommand(
            GetConnection(connectionName),
            logger,
            commandParser,
            analytics,
            assembly,
            resourcePath
        )
        {
            CommandTimeout = _defaultCommandTimeout,
        };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText)
    {
        return new SqlServerCommand(
            GetConnection(connectionName),
            logger,
            commandParser,
            analytics,
            commandName,
            commandText
        )
        {
            CommandTimeout = _defaultCommandTimeout,
        };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType)
    {
        return new SqlServerCommand(
            GetConnection(connectionName),
            logger,
            commandParser,
            analytics,
            commandName,
            commandType
        )
        {
            CommandTimeout = _defaultCommandTimeout,
        };
    }

    /// <inheritdoc />
    protected override DbConnection GetNewConnection(string datasourceName)
    {
        return new SqlConnection(_connectionSettings[datasourceName]);
    }
}
