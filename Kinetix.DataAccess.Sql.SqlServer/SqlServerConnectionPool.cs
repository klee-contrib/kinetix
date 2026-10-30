using System.Data;
using System.Data.Common;
using System.Reflection;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer;

/// <summary>
/// Pool de connections SQL Server.
/// </summary>
internal class SqlServerConnectionPool(
    TransactionScopeManager transactionScopeManager,
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
        return CreateSqlCommand(GetConnection(connectionName), procName);
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath)
    {
        return CreateSqlCommand(GetConnection(connectionName), assembly, resourcePath);
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText)
    {
        return CreateSqlCommand(GetConnection(connectionName), commandName, commandText);
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType)
    {
        return CreateSqlCommand(GetConnection(connectionName), commandName, commandType);
    }

    /// <inheritdoc />
    public override async Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string procName,
        CancellationToken ct = default
    )
    {
        return CreateSqlCommand(await GetConnectionAsync(connectionName, ct), procName);
    }

    /// <inheritdoc />
    public override async Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        Assembly assembly,
        string resourcePath,
        CancellationToken ct = default
    )
    {
        return CreateSqlCommand(await GetConnectionAsync(connectionName, ct), assembly, resourcePath);
    }

    /// <inheritdoc />
    public override async Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string commandName,
        string commandText,
        CancellationToken ct = default
    )
    {
        return CreateSqlCommand(await GetConnectionAsync(connectionName, ct), commandName, commandText);
    }

    /// <inheritdoc />
    public override async Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string commandName,
        CommandType commandType,
        CancellationToken ct = default
    )
    {
        return CreateSqlCommand(await GetConnectionAsync(connectionName, ct), commandName, commandType);
    }

    /// <inheritdoc />
    protected override DbConnection GetNewConnection(string datasourceName)
    {
        return new SqlConnection(_connectionSettings[datasourceName]);
    }

    private SqlServerCommand CreateSqlCommand(DbConnection connection, string procName)
    {
        return new SqlServerCommand(connection, logger, commandParser, procName)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private SqlServerCommand CreateSqlCommand(DbConnection connection, Assembly assembly, string resourcePath)
    {
        return new SqlServerCommand(connection, logger, commandParser, assembly, resourcePath)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private SqlServerCommand CreateSqlCommand(DbConnection connection, string commandName, string commandText)
    {
        return new SqlServerCommand(connection, logger, commandParser, commandName, commandText)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private SqlServerCommand CreateSqlCommand(DbConnection connection, string commandName, CommandType commandType)
    {
        return new SqlServerCommand(connection, logger, commandParser, commandName, commandType)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }
}
