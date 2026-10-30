using System.Data;
using System.Data.Common;
using System.Reflection;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Services;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Pool de connections SQL Server.
/// </summary>
internal class PostgresConnectionPool(
    TransactionScopeManager transactionScopeManager,
    CommandParser commandParser,
    ILogger<PostgresCommand> logger,
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
        return new NpgsqlConnection(_connectionSettings[datasourceName]);
    }

    private PostgresCommand CreateSqlCommand(DbConnection connection, string procName)
    {
        return new PostgresCommand(connection, logger, commandParser, procName)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private PostgresCommand CreateSqlCommand(DbConnection connection, Assembly assembly, string resourcePath)
    {
        return new PostgresCommand(connection, logger, commandParser, assembly, resourcePath)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private PostgresCommand CreateSqlCommand(DbConnection connection, string commandName, string commandText)
    {
        return new PostgresCommand(connection, logger, commandParser, commandName, commandText)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }

    private PostgresCommand CreateSqlCommand(DbConnection connection, string commandName, CommandType commandType)
    {
        return new PostgresCommand(connection, logger, commandParser, commandName, commandType)
        {
            CommandTimeout = _defaultCommandTimeout,
            InnerCommand = { Transaction = GetTransaction(connection) },
        };
    }
}
