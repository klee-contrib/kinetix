using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Kinetix.ComponentModel.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kinetix.Data.SqlClient
{
    /// <summary>
    /// Commande d'appel à SqlServer.
    /// </summary>
    public sealed class SqlServerCommand : IReadCommand
    {
        private const byte TimeOutErrorClass = 11;
        private const int TimeOutErrorCode1 = -2146232060;
        private const int TimeOutErrorCode2 = -2;

        private readonly SqlServerAnalytics _analytics;
        private readonly string _commandName;
        private readonly ILogger<SqlServerCommand> _logger;
        private readonly string _parserKey;

        private SqlServerConnection _connection;
        private IDbCommand _innerCommand;
        private SqlServerParameterCollection _parameterColl;

        internal SqlServerCommand(SqlServerConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, SqlServerAnalytics analytics, Assembly assembly, string resourcePath)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (string.IsNullOrEmpty(resourcePath))
            {
                throw new ArgumentNullException("resourcePath");
            }

            CommandParser = commandParser;
            _analytics = analytics;
            _commandName = resourcePath;
            _parserKey = resourcePath;
            _connection = connection;

            string commandText;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath)))
            {
                commandText = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(commandText))
            {
                throw new NotSupportedException(SR.ResourceNotFound);
            }

            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandType = CommandType.Text;
            _innerCommand.CommandText = commandText;
            _logger = logger;
        }

        internal SqlServerCommand(SqlServerConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, SqlServerAnalytics analytics, string procName)
        {
            CommandParser = commandParser;
            _analytics = analytics;
            _commandName = procName ?? throw new ArgumentNullException("procName");
            _connection = connection;
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandText = procName;
            _innerCommand.CommandType = CommandType.StoredProcedure;
            _logger = logger;
        }

        internal SqlServerCommand(SqlServerConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, SqlServerAnalytics analytics, string commandName, string commandText)
        {
            CommandParser = commandParser;
            _analytics = analytics;
            _commandName = commandName;
            _connection = connection;
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandText = commandText;
            _logger = logger;
        }

        internal SqlServerCommand(SqlServerConnection connection, ILogger<SqlServerCommand> logger, CommandParser commandParser, SqlServerAnalytics analytics, string commandName, CommandType commandType)
        {
            CommandParser = commandParser;
            _analytics = analytics;
            _commandName = commandName;
            _connection = connection;
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandType = commandType;
            _innerCommand.Connection = _connection.SqlConnection;
            _logger = logger;
        }

        private CommandParser CommandParser { get; }

        /// <summary>
        /// Obtient la commande SQL.
        /// </summary>
        public string CommandText
        {
            get => _innerCommand.CommandText;
            set => _innerCommand.CommandText = value;
        }

        /// <summary>
        /// Obtient ou définit le temps d'attente maximum pour l'exécution
        /// d'une commande (par défaut 30s).
        /// </summary>
        public int CommandTimeout
        {
            get => _innerCommand.CommandTimeout;
            set => _innerCommand.CommandTimeout = value;
        }

        /// <summary>
        /// Retourne la base de données utilisée.
        /// </summary>
        public string InitialCatalog
        {
            get
            {
                var connection = new SqlConnectionStringBuilder(_connection.SqlConnection.ConnectionString);
                return connection.InitialCatalog;
            }
        }

        /// <summary>
        /// Obtient ou définit le type de commande.
        /// </summary>
        public CommandType CommandType => _innerCommand.CommandType;

        /// <summary>
        /// Retourne la liste des paramétres de la commande.
        /// </summary>
        public SqlServerParameterCollection Parameters =>
            _parameterColl ??= new SqlServerParameterCollection(_innerCommand);

        /// <summary>
        /// Obtient ou définit les paramètres de la requête (limit, offset, tri).
        /// </summary>
        public QueryParameter QueryParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Retourne la liste des paramétres de la commande.
        /// </summary>
        IDataParameterCollection IReadCommand.Parameters => Parameters;

        /// <summary>
        /// Annule la commande.
        /// </summary>
        public void Cancel()
        {
            _innerCommand.Cancel();
        }

        /// <summary>
        /// Crée un nouveau paramétre pour la commande.
        /// </summary>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter CreateParameter()
        {
            return new SqlServerParameter(_innerCommand.CreateParameter());
        }

        /// <summary>
        /// Libère les ressources non managées.
        /// </summary>
        public void Dispose()
        {
            _innerCommand.Dispose();
            _innerCommand = null;
            _connection = null;
            _parameterColl = null;
        }

        /// <summary>
        /// Exécute la commande de mise à jour de données.
        /// </summary>
        /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
        /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
        /// <returns>Nombre de ligne impactées.</returns>
        public int ExecuteNonQuery(int minRowsAffected, int maxRowsAffected)
        {
            var rowsAffected = ExecuteNonQuery();
            if (rowsAffected < minRowsAffected)
            {
                throw rowsAffected == 0
                          ? new SqlServerException(SR.ExceptionZeroRowAffected)
                          : new SqlServerException(
                              string.Format(
                                  CultureInfo.CurrentCulture,
                                  SR.ExceptionTooFewRowsAffected,
                                  rowsAffected));
            }

            if (rowsAffected > maxRowsAffected)
            {
                throw new SqlServerException(string.Format(
                    CultureInfo.CurrentCulture,
                    SR.ExceptionTooManyRowsAffected,
                    rowsAffected));
            }

            return rowsAffected;
        }

        /// <summary>
        /// Exécute la commande de mise à jour de données.
        /// </summary>
        /// <returns>Nombre de ligne impactées.</returns>
        public int ExecuteNonQuery()
        {
            var listener = new SqlCommandListener(this, _analytics, _logger);
            try
            {
                CommandParser.ParseCommand(_innerCommand, _parserKey, null);
                return _innerCommand.ExecuteNonQuery();
            }
            catch (DbException sqle)
            {
                throw listener.HandleException(sqle);
            }
            finally
            {
                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <returns>DataReader.</returns>
        public SqlServerDataReader ExecuteReader()
        {
            var listener = new SqlCommandListener(this, _analytics, _logger);
            try
            {
                CommandParser.ParseCommand(_innerCommand, _parserKey, QueryParameters);
                return new SqlServerDataReader(_innerCommand.ExecuteReader(), QueryParameters);
            }
            catch (DbException sqle)
            {
                throw listener.HandleException(sqle);
            }
            finally
            {
                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <returns>DataReader.</returns>
        IDataReader IReadCommand.ExecuteReader()
        {
            return ExecuteReader();
        }

        /// <summary>
        /// Exécute une requête de select et retourne la première valeur
        /// de la première ligne.
        /// </summary>
        /// <returns>Retourne la valeur ou null.</returns>
        public object ExecuteScalar()
        {
            var listener = new SqlCommandListener(this, _analytics, _logger);
            try
            {
                CommandParser.ParseCommand(_innerCommand, _parserKey, null);
                var value = _innerCommand.ExecuteScalar();
                return (value == DBNull.Value) ? null : value;
            }
            catch (DbException sqle)
            {
                throw listener.HandleException(sqle);
            }
            finally
            {
                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une requête de select et retour la première valeur
        /// de la première ligne.
        /// </summary>
        /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
        /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
        /// <returns>Retourne la valeur ou null.</returns>
        public object ExecuteScalar(int minRowsAffected, int maxRowsAffected)
        {
            using var reader = ExecuteReader();
            if (reader.Read())
            {
                var rowsAffected = reader.RecordsAffected;
                if (rowsAffected > maxRowsAffected)
                {
                    throw new SqlServerException(string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionTooManyRowsAffected,
                        rowsAffected));
                }

                if (rowsAffected < minRowsAffected)
                {
                    throw new SqlServerException(string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionTooFewRowsAffected,
                        rowsAffected));
                }

                return reader.GetValue(0);
            }

            throw new SqlServerException(SR.ExceptionZeroRowAffected);
        }

        /// <summary>
        /// Classe permettant le suivi de l'éxécution des commandes.
        /// </summary>
        private sealed class SqlCommandListener : IDisposable
        {
            private static readonly string checkConstraintPattern = "CK_[A-Z_]*";
            private static readonly string foreignKeyConstraintPattern = "FK_[A-Z_]*";
            private static readonly string uniqueKeyConstraintPattern = "UK_[A-Z_]*";

            private readonly SqlServerAnalytics _analytics;
            private readonly SqlServerCommand _command;
            private readonly ILogger<SqlServerCommand> _logger;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="command">Commande.</param>
            public SqlCommandListener(SqlServerCommand command, SqlServerAnalytics analytics, ILogger<SqlServerCommand> logger)
            {
                _analytics = analytics;
                _command = command;
                _logger = logger;

                _analytics.StartCommand(_command._commandName);
            }

            /// <summary>
            /// Prend en charge une exception Sql Server.
            /// </summary>
            /// <param name="exception">Exception.</param>
            /// <returns>Exception.</returns>
            public Exception HandleException(DbException exception)
            {
                _analytics.CountError();

                if (!(exception is SqlException sqlException))
                {
                    return new SqlServerException(exception.Message, exception);
                }

                SqlErrorMessage message = null;
                foreach (SqlError error in sqlException.Errors)
                {
                    _logger.LogError($"Error class:{error.Class} message:{error.Message} line:{error.LineNumber} number:{error.Number} proc:{error.Procedure} server:{error.Server} source:{error.Source} state:{error.State}");

                    if (error.Number == 1205)
                    {
                        // Erreur de deadlock.
                        _analytics.CountDeadlock();
                    }
                    else if (error.Class == TimeOutErrorClass && (error.Number == TimeOutErrorCode1 || error.Number == TimeOutErrorCode2))
                    {
                        // Erreur de timeout.
                        _analytics.CountTimeout();
                        return new SqlServerTimeoutException(exception.Message, exception);
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

                return message != null
                    ? new BusinessException(message.Message, message.Code, exception)
                    : (Exception)new SqlServerException(exception.Message, exception);
            }

            /// <summary>
            /// Libère les ressources de la commande.
            /// </summary>
            public void Dispose()
            {
                var duration = _analytics.StopCommand();
                _logger.LogInformation($"{_command._commandName} ({duration} ms)");
                _logger.LogDebug(_command._innerCommand.CommandText);
                foreach (var parameter in _command.Parameters)
                {
                    if (parameter.Value is byte[] dataArray)
                    {
                        _logger.LogDebug($"{parameter.ParameterName} : byte[{dataArray.Length}]");
                    }
                    else
                    {
                        _logger.LogDebug($"{parameter.ParameterName}: {Convert.ToString(parameter.Value, CultureInfo.InvariantCulture)}");
                    }
                }

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Convertit une erreur de contrainte en message.
            /// </summary>
            /// <param name="initialMessage">Message initial.</param>
            /// <returns>Message final.</returns>
            /// <remarks>
            /// Prend en charge les messages 547 et 2601 dans les langues 1033 et 1036.
            /// </remarks>
            private SqlErrorMessage HandleConstraintException(string initialMessage)
            {
                var match = new Regex(foreignKeyConstraintPattern).Match(initialMessage);
                if (match.Success)
                {
                    return HandleForeignConstraintException(initialMessage, match.Value);
                }

                match = new Regex(checkConstraintPattern).Match(initialMessage);
                return match.Success
                    ? HandleCheckConstraintException(match.Value)
                    : null;
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
                            SqlServerConstraintViolation.ForeignKey :
                            SqlServerConstraintViolation.ReferenceKey;

                var message = _command.CommandParser.GetConstraintMessage(index, violation);
                if (string.IsNullOrEmpty(message))
                {
                    message = _command.CommandParser.GetConstraintMessage("FK_DEFAULT_MESSAGE", violation);
                }

                return new SqlErrorMessage(message, index);
            }

            /// <summary>
            /// Convertit une erreur de contrainte CHECK en message.
            /// </summary>
            /// <param name="constraintName">Nom de la contrainte.</param>
            /// <returns>Message final.</returns>
            private SqlErrorMessage HandleCheckConstraintException(string constraintName)
            {
                return new SqlErrorMessage(_command.CommandParser.GetConstraintMessage(constraintName, SqlServerConstraintViolation.Check), constraintName);
            }

            /// <summary>
            /// Convertit une erreur de contrainte en message.
            /// </summary>
            /// <param name="initialMessage">Message initial.</param>
            /// <returns>Message final.</returns>
            private SqlErrorMessage HandleUniqueConstraintException(string initialMessage)
            {
                var match = new Regex(uniqueKeyConstraintPattern).Match(initialMessage);
                if (!match.Success)
                {
                    return null;
                }

                var index = match.Value;
                return new SqlErrorMessage(_command.CommandParser.GetConstraintMessage(index, SqlServerConstraintViolation.Unique), index);
            }
        }

        /// <summary>
        /// Classe formalisant la remontée d'une erreur SQL une fois parsée.
        /// </summary>
        private sealed class SqlErrorMessage
        {
            /// <summary>
            /// Constructeur.
            /// </summary>
            /// <param name="message">Message d'erreur.</param>
            /// <param name="code">Code de l'erreur.</param>
            public SqlErrorMessage(string message, string code)
            {
                Message = message;
                Code = code;
            }

            /// <summary>
            /// Obtient le message d'erreur.
            /// </summary>
            public string Message
            {
                get;
                private set;
            }

            /// <summary>
            /// Obtient le code d'erreur.
            /// </summary>
            public string Code
            {
                get;
                private set;
            }
        }
    }
}
