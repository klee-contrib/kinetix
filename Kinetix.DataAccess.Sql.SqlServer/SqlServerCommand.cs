using System.Data;
using System.Reflection;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer;

/// <summary>
/// Commande d'appel à SQL.
/// </summary>
internal class SqlServerCommand : BaseSqlCommand
{
    private readonly AnalyticsManager _analytics;
    private readonly ILogger<SqlServerCommand> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="procName">Nom de la procédure stockée.</param>
    internal SqlServerCommand(IDbConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string procName)
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
    internal SqlServerCommand(IDbConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, AnalyticsManager analytics, Assembly assembly, string resourcePath)
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
    internal SqlServerCommand(IDbConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string commandName, string commandText)
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
    internal SqlServerCommand(IDbConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, AnalyticsManager analytics, string commandName, CommandType commandType)
        : base(connection, commandParser, commandName, commandType)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override SqlCommandListener GetSqlCommandListener()
    {
        return new SqlServerCommandListener(this, _analytics, _logger);
    }

    /// <inheritdoc />
    protected override SqlParameterCollection GetSqlParameterCollection()
    {
        return new SqlServerParameterCollection(InnerCommand);
    }
}
