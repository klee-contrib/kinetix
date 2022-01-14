using System;
using System.Data.Common;
using System.Data.SqlClient;
using Kinetix.ComponentModel.Exceptions;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer
{
    /// <summary>
    /// Classe permettant le suivi de l'éxécution des commandes.
    /// </summary>
    public class SqlServerCommandListener : SqlCommandListener
    {
        private const byte TimeOutErrorClass = 11;
        private const int TimeOutErrorCode1 = -2146232060;
        private const int TimeOutErrorCode2 = -2;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <param name="analytics">AnalyticsManager.</param>
        /// <param name="logger">Logger.</param>
        public SqlServerCommandListener(BaseSqlCommand command, AnalyticsManager analytics, ILogger<BaseSqlCommand> logger)
            : base(command, analytics, logger)
        {
        }

        /// <inheritdoc />
        public override Exception HandleException(DbException exception)
        {
            if (exception is not SqlException sqlException)
            {
                return new SqlDataException(exception.Message, exception);
            }

            SqlErrorMessage message = null;
            foreach (SqlError error in sqlException.Errors)
            {
                Logger.LogError($"Error class:{error.Class} message:{error.Message} line:{error.LineNumber} number:{error.Number} proc:{error.Procedure} server:{error.Server} source:{error.Source} state:{error.State}");

                if (error.Number == 1205)
                {
                    // Deadlock.
                }
                else if (error.Class == TimeOutErrorClass && (error.Number == TimeOutErrorCode1 || error.Number == TimeOutErrorCode2))
                {
                    return new SqlTimeoutException(exception.Message, exception);
                }
                else if (error.Class == 16 && error.Number == 547)
                {
                    // Erreur de contrainte.
                    message = HandleConstraintException(error.Message);
                }
                else if (error.Class == 14 && error.Number == 2601)
                {
                    // Erreur de contrainte.
                    message = HandleUniqueConstraintException(error.Message);
                }
                else if (error.Class == 14 && error.Number == 2627)
                {
                    // Erreur de contrainte.
                    message = HandleUniqueConstraintException(error.Message);
                }
            }

            Analytics.MarkProcessInError();
            return message != null
                ? new BusinessException(message.Message, message.Code, exception)
                : new SqlDataException(exception.Message, exception);
        }
    }
}
