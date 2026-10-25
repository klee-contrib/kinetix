using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Common;

/// <summary>
/// Classe permettant le suivi de l'éxécution des commandes.
/// </summary>
public abstract class SqlCommandListener : IDisposable
{
    public const string ActivitySourceName = "Kinetix.DataAccess.Sql";

    private const string CheckConstraintPattern = "CK_[A-Z_]*";
    private const string ForeignKeyConstraintPattern = "FK_[A-Z_]*";
    private const string UniqueKeyConstraintPattern = "UK_[A-Z_]*";

    private static readonly ActivitySource Source = new(ActivitySourceName);
    private readonly Activity? _activity;

    private readonly BaseSqlCommand _command;
    private readonly long _start = Stopwatch.GetTimestamp();
    private bool _disposed;

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="command">Commande.</param>
    /// <param name="logger">Logger.</param>
    protected SqlCommandListener(BaseSqlCommand command, ILogger<BaseSqlCommand> logger)
    {
        _command = command;
        Logger = logger;

        _activity = Source.StartActivity(
            command.CommandName,
            ActivityKind.Client,
            parentContext: default,
            tags: new ActivityTagsCollection
            {
                { "db.system.name", "sql" },
                { "db.namespace", command.Connection.Database },
                { "db.query.summary", command.CommandName },
                { "server.address", command.Connection.DataSource },
            }
        );
    }

    /// <summary>
    /// Logger.
    /// </summary>
    protected ILogger<BaseSqlCommand> Logger { get; }

    /// <summary>
    /// Libère les ressources de la commande.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Logger.LogInformation(
                "{CommandName} ({Duration} ms)",
                _command.CommandName,
                Stopwatch.GetElapsedTime(_start).TotalMilliseconds
            );
            Logger.LogDebug(_command.InnerCommand.CommandText);
            foreach (var parameter in _command.Parameters)
            {
                if (parameter.Value is byte[] dataArray)
                {
                    Logger.LogDebug($"{parameter.ParameterName} : byte[{dataArray.Length}]");
                }
                else
                {
                    Logger.LogDebug(
                        $"{parameter.ParameterName}: {Convert.ToString(parameter.Value, CultureInfo.InvariantCulture)}"
                    );
                }
            }
        }
        finally
        {
            _activity?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Prend en charge une exception Sql Server.
    /// </summary>
    /// <param name="exception">Exception.</param>
    /// <returns>Exception.</returns>
    public abstract Exception HandleException(DbException exception);

    /// <summary>
    /// Convertit une erreur de contrainte en message.
    /// </summary>
    /// <param name="initialMessage">Message initial.</param>
    /// <returns>Message final.</returns>
    /// <remarks>
    /// Prend en charge les messages 547 et 2601 dans les langues 1033 et 1036.
    /// </remarks>
    protected SqlErrorMessage? HandleConstraintException(string initialMessage)
    {
        var match = new Regex(ForeignKeyConstraintPattern).Match(initialMessage);
        if (match.Success)
        {
            return HandleForeignConstraintException(initialMessage, match.Value);
        }

        match = new Regex(CheckConstraintPattern).Match(initialMessage);
        return match.Success ? HandleCheckConstraintException(match.Value) : null;
    }

    /// <summary>
    /// Convertit une erreur de contrainte en message.
    /// </summary>
    /// <param name="initialMessage">Message initial.</param>
    /// <returns>Message final.</returns>
    protected SqlErrorMessage? HandleUniqueConstraintException(string initialMessage)
    {
        var match = new Regex(UniqueKeyConstraintPattern).Match(initialMessage);
        if (!match.Success)
        {
            return null;
        }

        var index = match.Value;
        return new SqlErrorMessage(
            _command.CommandParser.GetConstraintMessage(index, SqlConstraintViolation.Unique),
            index
        );
    }

    /// <summary>
    /// Enregistre l'échec de la commande dans l'activité courante de la commande.
    /// </summary>
    /// <param name="exception">Exception.</param>
    protected void RecordException(DbException exception)
    {
        _activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        _activity?.AddException(exception);
    }

    /// <summary>
    /// Convertit une erreur de contrainte CHECK en message.
    /// </summary>
    /// <param name="constraintName">Nom de la contrainte.</param>
    /// <returns>Message final.</returns>
    private SqlErrorMessage HandleCheckConstraintException(string constraintName)
    {
        return new SqlErrorMessage(
            _command.CommandParser.GetConstraintMessage(constraintName, SqlConstraintViolation.Check),
            constraintName
        );
    }

    /// <summary>
    /// Convertit une erreur de contrainte de clé étrangère en message.
    /// </summary>
    /// <param name="initialMessage">Message initial.</param>
    /// <param name="index">Nom de l'index.</param>
    /// <returns>Message final.</returns>
    private SqlErrorMessage HandleForeignConstraintException(string initialMessage, string index)
    {
        var violation = initialMessage.Contains("FOREIGN KEY")
            ? SqlConstraintViolation.ForeignKey
            : SqlConstraintViolation.ReferenceKey;

        var message = _command.CommandParser.GetConstraintMessage(index, violation);
        if (string.IsNullOrEmpty(message))
        {
            message = _command.CommandParser.GetConstraintMessage("FK_DEFAULT_MESSAGE", violation);
        }

        return new SqlErrorMessage(message, index);
    }
}
