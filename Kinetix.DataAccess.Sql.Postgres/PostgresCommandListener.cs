using System.Data.Common;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Modeling.Exceptions;
using Kinetix.Monitoring.Core;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Classe permettant le suivi de l'éxécution des commandes.
/// </summary>
/// <remarks>
/// Crée une nouvelle instance.
/// </remarks>
/// <param name="command">Commande.</param>
/// <param name="analytics">AnalyticsManager.</param>
/// <param name="logger">Logger.</param>
internal class PostgresCommandListener(
    BaseSqlCommand command,
    AnalyticsManager analytics,
    ILogger<BaseSqlCommand> logger
) : SqlCommandListener(command, analytics, logger)
{
    /// <inheritdoc />
    public override Exception HandleException(DbException exception)
    {
        if (exception is not PostgresException sqlException)
        {
            return new SqlDataException(exception.Message, exception);
        }

        SqlErrorMessage? message = null;

        Logger.LogError($"Error message:{sqlException.Message} file:{sqlException.File} source:{sqlException.Source}");

        if (sqlException.ConstraintName != null)
        {
            message =
                HandleConstraintException(sqlException.ConstraintName)
                ?? HandleUniqueConstraintException(sqlException.ConstraintName);
        }

        Analytics.MarkProcessInError();
        return message != null
            ? new BusinessException(message.Message, message.Code, exception)
            : new SqlDataException(exception.Message, exception);
    }
}
