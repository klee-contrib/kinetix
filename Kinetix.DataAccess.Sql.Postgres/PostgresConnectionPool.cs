using System.Data;
using System.Reflection;
using Kinetix.Monitoring;
using Kinetix.Services;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Pool de connections SQL Server.
/// </summary>
internal class PostgresConnectionPool : ConnectionPool
{
    private readonly AnalyticsManager _analytics;
    private readonly CommandParser _commandParser;
    private readonly Dictionary<string, string> _connectionSettings;
    private readonly int _defaultCommandTimeout;
    private readonly ILogger<PostgresCommand> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="transactionScopeManager">Composant injecté.</param>
    /// <param name="analytics">Composant injecté.</param>
    /// <param name="commandParser">Composant injecté.</param>
    /// <param name="config">Composant injecté.</param>
    /// <param name="logger">Composant injecté.</param>
    public PostgresConnectionPool(TransactionScopeManager transactionScopeManager, AnalyticsManager analytics, CommandParser commandParser, ILogger<PostgresCommand> logger, SqlConfig config)
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
        return new PostgresCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, procName) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath)
    {
        return new PostgresCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, assembly, resourcePath) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText)
    {
        return new PostgresCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandText) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    public override BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType)
    {
        return new PostgresCommand(GetConnection(connectionName), _logger, _commandParser, _analytics, commandName, commandType) { CommandTimeout = _defaultCommandTimeout };
    }

    /// <inheritdoc />
    protected override IDbConnection GetNewConnection(string datasourceName)
    {
        return new NpgsqlConnection(_connectionSettings[datasourceName]);
    }
}
