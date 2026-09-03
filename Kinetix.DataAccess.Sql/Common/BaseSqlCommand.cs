using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace Kinetix.DataAccess.Sql.Common;

/// <summary>
/// Commande d'appel à SQL.
/// </summary>
public abstract partial class BaseSqlCommand : IDisposable
{
    private readonly string? _parserKey;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="procName">Nom de la procédure stockée.</param>
    protected BaseSqlCommand(DbConnection connection, CommandParser commandParser, string procName)
    {
        CommandParser = commandParser;
        CommandName = procName ?? throw new ArgumentNullException(nameof(procName));
        Connection = connection;
        InnerCommand = Connection.CreateCommand();
        InnerCommand.CommandText = procName;
        InnerCommand.CommandType = CommandType.StoredProcedure;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="assembly">Assembly dans lequel chercher la requête SQL.</param>
    /// <param name="resourcePath">Chemin vers le fichier SQL.</param>
    protected BaseSqlCommand(
        DbConnection connection,
        CommandParser commandParser,
        Assembly assembly,
        string resourcePath
    )
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            throw new ArgumentNullException(nameof(resourcePath));
        }

        _parserKey = resourcePath;

        ArgumentNullException.ThrowIfNull(assembly);

        CommandParser = commandParser;
        CommandName = resourcePath;
        Connection = connection;

        string commandText;
        using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath)!))
        {
            commandText = reader.ReadToEnd();
        }

        if (string.IsNullOrEmpty(commandText))
        {
            throw new NotSupportedException(SR.ResourceNotFound);
        }

        InnerCommand = Connection.CreateCommand();
        InnerCommand.CommandType = CommandType.Text;
        InnerCommand.CommandText = commandText;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandText">Requête SQL.</param>
    protected BaseSqlCommand(
        DbConnection connection,
        CommandParser commandParser,
        string commandName,
        string commandText
    )
    {
        CommandParser = commandParser;
        CommandName = commandName;
        Connection = connection;
        InnerCommand = Connection.CreateCommand();
        InnerCommand.CommandText = commandText;
    }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connection">Connection SQL.</param>
    /// <param name="commandParser">Parser de requête.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandType">Type de la commande.</param>
    protected BaseSqlCommand(
        DbConnection connection,
        CommandParser commandParser,
        string commandName,
        CommandType commandType
    )
    {
        CommandParser = commandParser;
        CommandName = commandName;
        Connection = connection;
        InnerCommand = Connection.CreateCommand();
        InnerCommand.CommandType = commandType;
        InnerCommand.Connection = Connection;
    }

    /// <summary>
    /// Obtient la commande SQL.
    /// </summary>
    public string CommandText
    {
        get => InnerCommand.CommandText;
        set => InnerCommand.CommandText = value;
    }

    /// <summary>
    /// Obtient ou définit le temps d'attente maximum pour l'exécution
    /// d'une commande (par défaut 30s).
    /// </summary>
    public int CommandTimeout
    {
        get => InnerCommand.CommandTimeout;
        set => InnerCommand.CommandTimeout = value;
    }

    /// <summary>
    /// Obtient ou définit le type de commande.
    /// </summary>
    public CommandType CommandType => InnerCommand.CommandType;

    /// <summary>
    /// Retourne la liste des paramétres de la commande.
    /// </summary>
    public SqlParameterCollection Parameters => field ??= GetSqlParameterCollection();

    /// <summary>
    /// Obtient ou définit les paramètres de la requête (limit, offset, tri).
    /// </summary>
    public QueryParameter? QueryParameters { get; set; }

    /// <summary>
    /// Commande SQL.
    /// </summary>
    public DbCommand InnerCommand { get; private set; }

    /// <summary>
    /// Parser de commande.
    /// </summary>
    internal CommandParser CommandParser { get; }

    /// <summary>
    /// Nom de la commande.
    /// </summary>
    internal string CommandName { get; }

    /// <summary>
    /// Connexion SQL.
    /// </summary>
    internal DbConnection Connection { get; private set; }

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des entiers.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des entiers à insérer dans le IN.</param>
    /// <returns>La commande.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public BaseSqlCommand AddInParameter(string parameterName, IEnumerable<int> list)
    {
        Parameters.AddInParameter(parameterName, list);
        return this;
    }

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des chaines de caractères.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des strings à insérer dans le IN.</param>
    /// <returns>Le paramètre créé.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public BaseSqlCommand AddInParameter(string parameterName, IEnumerable<string> list)
    {
        Parameters.AddInParameter(parameterName, list);
        return this;
    }

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des chaines de caractères.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des guids à insérer dans le IN.</param>
    /// <returns>Le paramètre créé.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public BaseSqlCommand AddInParameter(string parameterName, IEnumerable<Guid> list)
    {
        Parameters.AddInParameter(parameterName, list);
        return this;
    }

    /// <summary>
    /// Ajout un nouveau paramètre à partir d'une colonne et de sa valeur.
    /// Le paramètre est un paramètre d'entrée.
    /// </summary>
    /// <param name="colName">Colonnne du paramètre.</param>
    /// <param name="value">Valeur du paramètre.</param>
    /// <returns>Commande.</returns>
    public BaseSqlCommand AddParameter(Enum colName, object value)
    {
        Parameters.AddWithValue(colName, value);
        return this;
    }

    /// <summary>
    /// Ajout un nouveau paramètre à partir de son nom et de sa valeur.
    /// Le paramètre est un paramètre d'entrée.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <param name="value">Valeur du paramètre.</param>
    /// <returns>Commande.</returns>
    public BaseSqlCommand AddParameter(string parameterName, object? value)
    {
        Parameters.AddWithValue(parameterName, value);
        return this;
    }

    /// <summary>
    /// Ajoute une liste de bean en paramètre (La colonne InsertKey est obligatoire).
    /// </summary>
    /// <typeparam name="T">Type du bean.</typeparam>
    /// <param name="collection">Collection à passer en paramètre.</param>
    /// <returns>La commande.</returns>
    public BaseSqlCommand AddTableParameter<T>(ICollection<T> collection)
        where T : class, new()
    {
        Parameters.AddTableParameter(collection);
        return this;
    }

    /// <summary>
    /// Annule la commande.
    /// </summary>
    public void Cancel()
    {
        InnerCommand.Cancel();
    }

    /// <summary>
    /// Libère les ressources non managées.
    /// </summary>
    public void Dispose()
    {
        InnerCommand.Dispose();
    }

    /// <summary>
    /// Exécute la commande de mise à jour de données.
    /// </summary>
    /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
    /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Nombre de ligne impactées.</returns>
    public async Task<int> ExecuteNonQueryAsync(
        int minRowsAffected,
        int maxRowsAffected,
        CancellationToken ct = default
    )
    {
        var rowsAffected = await ExecuteNonQueryAsync(ct);
        if (rowsAffected < minRowsAffected)
        {
            throw rowsAffected == 0
                ? new SqlDataException(SR.ExceptionZeroRowAffected)
                : new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooFewRowsAffected, rowsAffected)
                );
        }

        if (rowsAffected > maxRowsAffected)
        {
            throw new SqlDataException(
                string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooManyRowsAffected, rowsAffected)
            );
        }

        return rowsAffected;
    }

    /// <summary>
    /// Exécute la commande de mise à jour de données.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Nombre de ligne impactées.</returns>
    public async Task<int> ExecuteNonQueryAsync(CancellationToken ct = default)
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, queryParameter: null);
            return await InnerCommand.ExecuteNonQueryAsync(ct);
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
    /// <param name="ct">CancellationToken.</param>
    /// <returns>DataReader.</returns>
    public async Task<SqlDataReader> ExecuteReaderAsync(CancellationToken ct = default)
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, QueryParameters);
            return new SqlDataReader(await InnerCommand.ExecuteReaderAsync(ct), QueryParameters);
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
    /// Exécute une requête de select et retourne la première valeur
    /// de la première ligne.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Retourne la valeur ou null.</returns>
    public async Task<object?> ExecuteScalarAsync(CancellationToken ct = default)
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, queryParameter: null);
            var value = await InnerCommand.ExecuteScalarAsync(ct);
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
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Retourne la valeur ou null.</returns>
    public async Task<object?> ExecuteScalarAsync(
        int minRowsAffected,
        int maxRowsAffected,
        CancellationToken ct = default
    )
    {
        using var reader = await ExecuteReaderAsync(ct);
        if (reader.Read())
        {
            var rowsAffected = reader.RecordsAffected;
            if (rowsAffected > maxRowsAffected)
            {
                throw new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooManyRowsAffected, rowsAffected)
                );
            }

            if (rowsAffected < minRowsAffected)
            {
                throw new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooFewRowsAffected, rowsAffected)
                );
            }

            return reader.GetValue(0);
        }

        throw new SqlDataException(SR.ExceptionZeroRowAffected);
    }

    /// <summary>
    /// Crée un nouveau paramètre pour la commande.
    /// </summary>
    /// <returns>Paramètre.</returns>
    internal SqlDataParameter CreateParameter()
    {
        return new SqlDataParameter(InnerCommand.CreateParameter());
    }

    /// <summary>
    /// Récupère un nouveau SqlCommandListener.
    /// </summary>
    /// <returns>SqlCommandListener.</returns>
    protected abstract SqlCommandListener GetSqlCommandListener();

    /// <summary>
    /// Récupère une nouvelle SqlParameterCollection.
    /// </summary>
    /// <returns>SqlParameterCollection.</returns>
    protected abstract SqlParameterCollection GetSqlParameterCollection();
}
