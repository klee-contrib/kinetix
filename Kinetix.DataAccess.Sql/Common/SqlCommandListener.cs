using System;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Classe permettant le suivi de l'éxécution des commandes.
    /// </summary>
    public abstract class SqlCommandListener : IDisposable
    {
        private const string CheckConstraintPattern = "CK_[A-Z_]*";
        private const string ForeignKeyConstraintPattern = "FK_[A-Z_]*";
        private const string UniqueKeyConstraintPattern = "UK_[A-Z_]*";

        private readonly BaseSqlCommand _command;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <param name="analytics">AnalyticsManager.</param>
        /// <param name="logger">Logger.</param>
        public SqlCommandListener(BaseSqlCommand command, AnalyticsManager analytics, ILogger<BaseSqlCommand> logger)
        {
            Analytics = analytics;
            _command = command;
            Logger = logger;

            Analytics.StartProcess(_command.CommandName, "Database", _command.Connection.Database);
        }

        /// <summary>
        /// Analytics.
        /// </summary>
        protected AnalyticsManager Analytics { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger<BaseSqlCommand> Logger { get; }

        /// <summary>
        /// Libère les ressources de la commande.
        /// </summary>
        public void Dispose()
        {
            var process = Analytics.StopProcess();
            if (!process.Disabled)
            {
                Logger.LogInformation($"{_command.CommandName} ({process.Duration} ms)");
                Logger.LogDebug(_command.InnerCommand.CommandText);
                foreach (var parameter in _command.Parameters)
                {
                    if (parameter.Value is byte[] dataArray)
                    {
                        Logger.LogDebug($"{parameter.ParameterName} : byte[{dataArray.Length}]");
                    }
                    else
                    {
                        Logger.LogDebug($"{parameter.ParameterName}: {Convert.ToString(parameter.Value, CultureInfo.InvariantCulture)}");
                    }
                }
            }

            GC.SuppressFinalize(this);
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
        protected SqlErrorMessage HandleConstraintException(string initialMessage)
        {
            var match = new Regex(ForeignKeyConstraintPattern).Match(initialMessage);
            if (match.Success)
            {
                return HandleForeignConstraintException(initialMessage, match.Value);
            }

            match = new Regex(CheckConstraintPattern).Match(initialMessage);
            return match.Success
                ? HandleCheckConstraintException(match.Value)
                : null;
        }

        /// <summary>
        /// Convertit une erreur de contrainte en message.
        /// </summary>
        /// <param name="initialMessage">Message initial.</param>
        /// <returns>Message final.</returns>
        protected SqlErrorMessage HandleUniqueConstraintException(string initialMessage)
        {
            var match = new Regex(UniqueKeyConstraintPattern).Match(initialMessage);
            if (!match.Success)
            {
                return null;
            }

            var index = match.Value;
            return new SqlErrorMessage(_command.CommandParser.GetConstraintMessage(index, SqlConstraintViolation.Unique), index);
        }

        /// <summary>
        /// Convertit une erreur de contrainte CHECK en message.
        /// </summary>
        /// <param name="constraintName">Nom de la contrainte.</param>
        /// <returns>Message final.</returns>
        private SqlErrorMessage HandleCheckConstraintException(string constraintName)
        {
            return new SqlErrorMessage(_command.CommandParser.GetConstraintMessage(constraintName, SqlConstraintViolation.Check), constraintName);
        }

        /// <summary>
        /// Convertit une erreur de contrainte de clé étrangère en message.
        /// </summary>
        /// <param name="initialMessage">Message initial.</param>
        /// <param name="index">Nom de l'index.</param>
        /// <returns>Message final.</returns>
        private SqlErrorMessage HandleForeignConstraintException(string initialMessage, string index)
        {
            var violation =
                        initialMessage.Contains("FOREIGN KEY") ?
                        SqlConstraintViolation.ForeignKey :
                        SqlConstraintViolation.ReferenceKey;

            var message = _command.CommandParser.GetConstraintMessage(index, violation);
            if (string.IsNullOrEmpty(message))
            {
                message = _command.CommandParser.GetConstraintMessage("FK_DEFAULT_MESSAGE", violation);
            }

            return new SqlErrorMessage(message, index);
        }
    }
}
