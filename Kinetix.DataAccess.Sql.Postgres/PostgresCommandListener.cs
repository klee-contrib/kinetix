using System.Data.Common;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Modeling.Exceptions;
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
/// <param name="logger">Logger.</param>
internal class PostgresCommandListener(BaseSqlCommand command, ILogger<BaseSqlCommand> logger)
    : SqlCommandListener(command, logger)
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

        RecordException(exception);
        return message != null
            ? new BusinessException(message.Message, message.Code, exception)
            : new SqlDataException(exception.Message, exception);
    }
}
