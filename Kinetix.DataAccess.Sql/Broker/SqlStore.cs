using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Text;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Modeling;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Store de base pour le stockage en base de données.
/// </summary>
/// <typeparam name="T">Type du store.</typeparam>
public abstract partial class SqlStore<T> : IStore<T>
    where T : class, new()
{
    /// <summary>
    /// Préfixe générique d'un service de suppression.
    /// </summary>
    private const string ServiceDelete = "SV_DELETE";

    /// <summary>
    /// Préfixe générique d'un service d'insertion.
    /// </summary>
    private const string ServiceInsert = "SV_INSERT";

    /// <summary>
    /// Préfixe générique d'un service de sélection.
    /// </summary>
    private const string ServiceSelect = "SV_SELECT";

    /// <summary>
    /// Préfixe générique d'un service de mise à jour.
    /// </summary>
    private const string ServiceUpdate = "SV_UPDATE";

    private readonly ILogger<BrokerManager> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
    /// <param name="connectionPool">Pool de connexions.</param>
    /// <param name="logger">Logger.</param>
    protected SqlStore(string dataSourceName, ConnectionPool connectionPool, ILogger<BrokerManager> logger)
    {
        _logger = logger;

        try
        {
            // Charge la définition de l'objet donné T.
            Definition = BeanDescriptor.GetDefinition(typeof(T), ignoreCustomTypeDesc: true);
            ConnectionPool = connectionPool;

            var attrs = typeof(T).GetCustomAttributes(typeof(TableAttribute), inherit: true);
            if (attrs == null || attrs.Length == 0)
            {
                throw new NotSupportedException(typeof(T).FullName + " has no TableAttribute. Check type persistence.");
            }

            if (string.IsNullOrEmpty(Definition.ContractName))
            {
                throw new NotSupportedException(typeof(T) + " has no ContractName defined. Check type persistence.");
            }

            if (Definition.PrimaryKey == null)
            {
                throw new NotSupportedException(typeof(T) + " has no primary key defined.");
            }

            DataSourceName = dataSourceName ?? throw new ArgumentNullException(nameof(dataSourceName));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Echec d'instanciation du store.");
            throw new BrokerException("Broker<" + typeof(T).FullName + "> " + e.Message, e);
        }
    }

    /// <summary>
    /// Current user logging statement.
    /// </summary>
    protected string? CurrentUserStatementLog { get; set; }

    /// <summary>
    /// Source de données du store.
    /// </summary>
    protected string DataSourceName { get; }

    /// <summary>
    /// Retourne la définition.
    /// </summary>
    protected BeanDefinition Definition { get; }

    /// <summary>
    /// Pool de connexion.
    /// </summary>
    protected ConnectionPool ConnectionPool { get; }

    /// <summary>
    /// Lancemement d'une exception si la requête retourne un nombre de lignes supérieur au maximum spécifié.
    /// </summary>
    protected virtual bool ThrowExceptionOnRowOverflow => true;

    /// <summary>
    /// Préfixe utilisé par le store pour faire référence à une variable.
    /// </summary>
    protected abstract string VariablePrefix { get; }

    /// <summary>
    /// Caractère de conacténation.
    /// </summary>
    protected abstract string ConcatCharacter { get; }

    /// <inheritdoc cref="IStore{T}.LoadAllAsync" />
    public async Task<IList<T>> LoadAllAsync(QueryParameter? queryParameter, CancellationToken ct = default)
    {
        var commandName = ServiceSelect + "_ALL_" + Definition.ContractName;
        return await InternalLoadAllAsync(commandName, queryParameter, new FilterCriteria(), ct);
    }

    /// <inheritdoc cref="IStore{T}.LoadAllByCriteriaAsync" />
    public async Task<IList<T>> LoadAllByCriteriaAsync(
        FilterCriteria criteria,
        QueryParameter? queryParameter,
        CancellationToken ct = default
    )
    {
        // Les critères ne doivent pas être vides
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceSelect + "_ALL_LIKE_" + Definition.ContractName;

        return await InternalLoadAllAsync(commandName, queryParameter, criteria, ct);
    }

    /// <inheritdoc cref="IStore{T}.LoadAsync" />
    public async Task<T?> LoadAsync(object primaryKey, CancellationToken ct = default)
    {
        return await CollectionBuilder<T>.ParseCommandForSingleObjectAsync(GetLoadCommand(primaryKey), ct: ct);
    }

    /// <inheritdoc cref="IStore{T}.LoadByCriteriaAsync" />
    public async Task<T?> LoadByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceSelect + "_LIKE_" + Definition.ContractName;
        var cmd = GetCommand(commandName, Definition.ContractName!, criteria, queryParameter: null);
        return await CollectionBuilder<T>.ParseCommandForSingleObjectAsync(cmd, ct: ct);
    }

    /// <inheritdoc cref="IStore{T}.PutAllAsync" />
    public async Task<ICollection<T>> PutAllAsync(ICollection<T> collection, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0)
        {
            return collection;
        }

        var commandName = ServiceInsert + "_" + Definition.ContractName;
        return await InsertAllAsync(commandName, collection, Definition, ct);
    }

    /// <inheritdoc cref="IStore{T}.PutAsync" />
    public async Task<object> PutAsync(
        T bean,
        bool forceInsert,
        ColumnSelector? columnSelector = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(bean);

        BeanDescriptor.Check(
            bean,
            columnSelector != null
                ? Definition
                    .Properties.Where(p => columnSelector.ColumnList.Contains(p.MemberName!))
                    .Select(p => p.PropertyName)
                : null
        );

        var value = Definition.PrimaryKey.GetValue(bean);

        using var reader = await GetPutCommand(bean, value, forceInsert, columnSelector).ExecuteReaderAsync(ct);

        if (reader.RecordsAffected == 0)
        {
            throw new BrokerException("Zero record affected");
        }

        if (reader.RecordsAffected > 1)
        {
            throw new BrokerException("Too many records affected");
        }

        // Dans le cas d'un update, il n'y a plus de select
        // qui compte le nombre de lignes mises à jour, donc
        // on retourne directement l'identifiant.
        if (value != null)
        {
            return value;
        }

        reader.Read();
        return reader.GetValue(0)!;
    }

    /// <inheritdoc cref="IStore{T}.RemoveAllByCriteriaAsync" />
    public async Task RemoveAllByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceDelete + "_ALL_LIKE_" + Definition.ContractName;
        await InternalDeleteAllByCriteriaAsync(commandName, Definition.ContractName!, criteria, ct);
    }

    /// <inheritdoc cref="IStore{T}.RemoveAsync" />
    public async Task RemoveAsync(object primaryKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        Definition.PrimaryKey.CheckValueType(primaryKey);
        var commandName = ServiceDelete + "_" + Definition.ContractName;

        // On charge l'objet à partir d'un seul critère
        // correspondant à sa clé primaire
        var criteria = new FilterCriteria(Definition.PrimaryKey.MemberName!, Expression.Equals, primaryKey);

        var rowsAffected = await InternalDeleteAllByCriteriaAsync(commandName, Definition.ContractName!, criteria, ct);
        if (rowsAffected == 0)
        {
            throw new BrokerException("Zero row deleted");
        }

        if (rowsAffected > 1)
        {
            throw new BrokerException("Too many rows deleted");
        }
    }

    /// <summary>
    /// Crée la requête SQL d'insertion d'un bean d'un bean.
    /// </summary>
    /// <param name="beanDefinition">Définition du bean.</param>
    /// <param name="isGeneratedPK">PK autogénérée ou non.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
    /// <returns>Query.</returns>
    protected abstract string BuildInsertQuery(
        BeanDefinition beanDefinition,
        bool isGeneratedPK,
        ColumnSelector? columnSelector
    );

    /// <summary>
    /// Dépose les beans dans le store.
    /// </summary>
    /// <param name="commandName">Nom du service.</param>
    /// <param name="collection">Beans à enregistrer.</param>
    /// <param name="beanDefinition">Définition.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Beans enregistrés.</returns>
    protected abstract Task<ICollection<T>> InsertAllAsync(
        string commandName,
        ICollection<T> collection,
        BeanDefinition beanDefinition,
        CancellationToken ct = default
    );

    private BaseSqlCommand GetCommand(
        string commandName,
        string tableName,
        FilterCriteria criteria,
        QueryParameter? queryParameter
    )
    {
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        command.QueryParameters = queryParameter;

        var commandText = new StringBuilder("select ");

        string? order = null;
        if (queryParameter != null && !string.IsNullOrEmpty(queryParameter.SortCondition))
        {
            order = queryParameter.SortCondition;
        }

        var properties = BeanDescriptor.GetDefinition(typeof(T)).Properties;
        var hasColumn = false;
        foreach (var property in properties)
        {
            if (string.IsNullOrEmpty(property.MemberName))
            {
                continue;
            }

            if (property.PropertyType == typeof(byte[]))
            {
                continue;
            }

            if (hasColumn)
            {
                commandText.Append(", ");
            }

            commandText.Append(property.MemberName);
            hasColumn = true;
        }

        commandText.Append(" from ").Append(tableName);

        PrepareFilterCriteria(criteria, command, commandText);

        // Ajout du Order By si non-nul
        if (!string.IsNullOrEmpty(order))
        {
            commandText.Append(" order by ");
            commandText.Append(order);
        }

        // Set de la requête
        command.CommandText = commandText.ToString();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Exécution de la requête '{commandName}' : {command.CommandText}");
        }

        return command;
    }

    private BaseSqlCommand GetDeleteCommand(string commandName, string tableName, FilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        command.CommandTimeout = 0;
        var commandText = new StringBuilder(CurrentUserStatementLog);
        commandText.Append("delete from ");
        commandText.Append(tableName);
        if (criteria.Parameters.Any())
        {
            PrepareFilterCriteria(criteria, command, commandText);
        }

        command.CommandText = commandText.ToString();
        return command;
    }

    private BaseSqlCommand GetLoadAllCommand(
        string commandName,
        QueryParameter? queryParameter,
        FilterCriteria criteria
    )
    {
        // Définition du tri à partir de la requete.
        queryParameter?.RemapSortColumn(typeof(T));
        return GetCommand(commandName, Definition.ContractName!, criteria, queryParameter);
    }

    private BaseSqlCommand GetLoadCommand(object primaryKey)
    {
        ArgumentNullException.ThrowIfNull(primaryKey);

        Definition.PrimaryKey.CheckValueType(primaryKey);

        var commandName = ServiceSelect + "_" + Definition.ContractName;

        // On charge l'objet à partir d'un seul critère correspondant à sa clé primaire
        var criteria = new FilterCriteria(Definition.PrimaryKey.MemberName!, Expression.Equals, primaryKey);

        return GetCommand(commandName, Definition.ContractName!, criteria, queryParameter: null);
    }

    private BaseSqlCommand GetPutCommand(
        T bean,
        object? primaryKeyValue,
        bool forceInsert,
        ColumnSelector? columnSelector
    )
    {
        if (!forceInsert && primaryKeyValue != null)
        {
            var commandName = ServiceUpdate + "_" + Definition.ContractName;

            var sbUpdate = new StringBuilder(CurrentUserStatementLog);
            sbUpdate.Append("update ");

            var sbUpdateSet = new StringBuilder(Definition.ContractName);
            sbUpdateSet.Append(" set");

            var sbUpdateWhere = new StringBuilder(" where ");
            sbUpdateWhere
                .Append(Definition.PrimaryKey.MemberName)
                .Append(" = ")
                .Append(VariablePrefix)
                .Append(Definition.PrimaryKey.MemberName);

            // Construction des champs de l'update SET et du WHERE
            var count = 0;
            foreach (var property in Definition.Properties)
            {
                // Si la propriété est une clé primaire ou n'est pas défini,
                // on passe à la propriété suivante.
                if (
                    property.MemberName == null
                    || property.IsPrimaryKey
                    || property.IsReadOnly
                    || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName)
                )
                {
                    continue;
                }

                if (count > 0)
                {
                    sbUpdateSet.Append(',');
                }

                sbUpdateSet.Append(' ').Append(property.MemberName).Append(" = ");

                // Insertion de la valeur à mettre à jour
                sbUpdateSet.Append(VariablePrefix).Append(property.MemberName);
                count++;
            }

            sbUpdate.Append(sbUpdateSet).Append(sbUpdateWhere);
            var sql = sbUpdate.ToString();

            var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, sql);
            command.CommandTimeout = 0;

            foreach (var property in Definition.Properties)
            {
                if (
                    property.MemberName == null
                    || columnSelector != null
                        && !columnSelector.ColumnList.Contains(property.MemberName)
                        && !property.IsPrimaryKey
                )
                {
                    continue;
                }

                var value = property.GetValue(bean);

                if (property.IsPrimaryKey)
                {
                    command.Parameters.AddWithValue("PK_" + property.MemberName, value);
                }

                // Ajout du paramètre en entrée de la commande envoyée à SQL Server.
                var parameter = command.Parameters.AddWithValue(property.MemberName!, value);
                if (property.PrimitiveType == typeof(byte[]))
                {
                    parameter.DbType = DbType.Binary;
                }
            }

            return command;
        }
        else
        {
            var commandName = ServiceInsert + "_" + Definition.ContractName;
            var sql = BuildInsertQuery(Definition, primaryKeyValue == null, columnSelector);
            var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, sql);
            command.CommandTimeout = 0;

            foreach (var property in Definition.Properties)
            {
                if (property == Definition.PrimaryKey && primaryKeyValue == null)
                {
                    continue;
                }

                if (
                    property.MemberName == null
                    || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName)
                )
                {
                    continue;
                }

                var value = property.GetValue(bean);

                // Ajout du paramètre en entrée de la commande.
                var parameter = command.Parameters.AddWithValue(property.MemberName!, value);
                if (property.PrimitiveType == typeof(byte[]))
                {
                    parameter.DbType = DbType.Binary;
                }
            }

            return command;
        }
    }

    private async Task<int> InternalDeleteAllByCriteriaAsync(
        string commandName,
        string tableName,
        FilterCriteria criteria,
        CancellationToken ct = default
    )
    {
        return await GetDeleteCommand(commandName, tableName, criteria).ExecuteNonQueryAsync(ct);
    }

    private async Task<List<T>> InternalLoadAllAsync(
        string commandName,
        QueryParameter? queryParameter,
        FilterCriteria criteria,
        CancellationToken ct = default
    )
    {
        return await CollectionBuilder<T>
            .ParseCommandAsync(GetLoadAllCommand(commandName, queryParameter, criteria), ct)
            .ToListAsync(cancellationToken: ct);
    }

    private void PrepareFilterCriteria(FilterCriteria filter, BaseSqlCommand command, StringBuilder commandText)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(commandText);

        var pos = 0;
        var mapParameters = new Dictionary<string, int>();
        foreach (var criteriaParam in filter.Parameters)
        {
            commandText.Append(pos == 0 ? " where " : " and ");
            commandText.Append(criteriaParam.ColumnName);

            string? parameterName = null;
            if (!mapParameters.TryGetValue(criteriaParam.ColumnName, out var value))
            {
                parameterName = criteriaParam.ColumnName;
                mapParameters.Add(criteriaParam.ColumnName, 1);
            }
            else
            {
                mapParameters[criteriaParam.ColumnName] = value + 1;
                parameterName =
                    criteriaParam.ColumnName
                    + mapParameters[criteriaParam.ColumnName].ToString(CultureInfo.InvariantCulture);
            }

            if (criteriaParam.Expression == Expression.Between)
            {
                var dateValues = (DateTime[])criteriaParam.Value!;
                command.AddParameter(parameterName + "T1", dateValues[0]);
                command.AddParameter(parameterName + "T2", dateValues[1]);
            }
            else
            {
                command.AddParameter(parameterName, criteriaParam.Value);
            }

            commandText.Append(
                criteriaParam.Expression switch
                {
                    Expression.Between => " BETWEEN "
                        + VariablePrefix
                        + parameterName
                        + "T1"
                        + " AND "
                        + VariablePrefix
                        + parameterName
                        + "T2",
                    Expression.Contains => " LIKE '%' + "
                        + VariablePrefix
                        + parameterName
                        + " "
                        + ConcatCharacter
                        + " '%'",
                    Expression.EndsWith => " LIKE '%' + " + VariablePrefix + parameterName,
                    Expression.Equals => " = " + VariablePrefix + parameterName,
                    Expression.GreaterOrEquals => " >= " + VariablePrefix + parameterName,
                    Expression.LowerOrEquals => " <= " + VariablePrefix + parameterName,
                    Expression.Greater => " > " + VariablePrefix + parameterName,
                    Expression.IsNotNull => " IS NOT NULL",
                    Expression.IsNull => " IS NULL",
                    Expression.Lower => " < " + VariablePrefix + parameterName,
                    Expression.NotStartsWith => " NOT LIKE "
                        + VariablePrefix
                        + parameterName
                        + " "
                        + ConcatCharacter
                        + "'%'",
                    Expression.StartsWith => " LIKE " + VariablePrefix + parameterName + " " + ConcatCharacter + " '%'",
                    Expression.NotEquals => " != " + VariablePrefix + parameterName,
                    _ => throw new NotSupportedException(
                        "Type d'expression de filtre non supportée : " + criteriaParam.Expression.ToString()
                    ),
                }
            );
            ++pos;
        }
    }
}
