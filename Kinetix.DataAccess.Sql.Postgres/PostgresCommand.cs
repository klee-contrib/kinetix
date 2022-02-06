using System.Data;
using System.Reflection;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Commande d'appel à SQL.
/// </summary>
internal class PostgresCommand : BaseSqlCommand
{
    private readonly AnalyticsManager _analytics;
    private readonly ILogger<PostgresCommand> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="procName">Nom de la procédure stockée.</param>
    internal PostgresCommand(IDbConnection connection, ILogger<PostgresCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string procName)
        : base(connection, commandParser, procName)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="assembly">Assembly dans lequel chercher la requête SQL.</param>
    /// <param name="resourcePath">Chemin vers le fichier SQL.</param>
    internal PostgresCommand(IDbConnection connection, ILogger<PostgresCommand> logger, CommandParser commandParser, AnalyticsManager analytics, Assembly assembly, string resourcePath)
        : base(connection, commandParser, assembly, resourcePath)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandText">Requête SQL.</param>
    internal PostgresCommand(IDbConnection connection, ILogger<PostgresCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string commandName, string commandText)
        : base(connection, commandParser, commandName, commandText)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandType">Type de la commande.</param>
    internal PostgresCommand(IDbConnection connection, ILogger<PostgresCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string commandName, CommandType commandType)
        : base(connection, commandParser, commandName, commandType)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override SqlCommandListener GetSqlCommandListener()
    {
        return new PostgresCommandListener(this, _analytics, _logger);
    }

    /// <inheritdoc />
    protected override SqlParameterCollection GetSqlParameterCollection()
    {
        return new PostgresParameterCollection(InnerCommand);
    }
}
