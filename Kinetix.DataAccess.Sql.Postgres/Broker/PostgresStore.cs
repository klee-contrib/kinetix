using System.Data;
using System.Text;
using Kinetix.Modeling;
using Kinetix.DataAccess.Sql.Broker;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Postgres.Broker;

/// <summary>
/// Store de base pour le stockage en base de données.
/// </summary>
/// <typeparam name="T">Type du store.</typeparam>
internal class PostgresStore<T> : SqlStore<T>
    where T : class, new()
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
    /// <param name="connectionPool">Pool de connexions.</param>
    /// <param name="logger">Logger.</param>
    public PostgresStore(string dataSourceName, ConnectionPool connectionPool, ILogger<BrokerManager> logger)
        : base(dataSourceName, connectionPool, logger)
    {
    }

    /// <inheritdoc />
    protected override string VariablePrefix => "@";

    /// <inheritdoc />
    protected override string ConcatCharacter => " + ";

    /// <inheritdoc />
    protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector)
    {
        var sql = BuildInsertQuery(beanDefinition, true, columnSelector);
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, sql);
        command.CommandTimeout = 0;
        AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
        return command.ExecuteReader();
    }

    /// <inheritdoc />
    protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue)
    {
        if (primaryKey == null)
        {
            throw new ArgumentNullException("primaryKey");
        }

        var sql = BuildInsertQuery(beanDefinition, true, columnSelector);
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, sql);
        command.CommandTimeout = 0;
        command.AddParameter(primaryKey.MemberName, primaryKeyValue);
        AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
        return command.ExecuteReader();
    }

    /// <inheritdoc />
    protected override ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition)
    {
        if (collection == null)
        {
            throw new ArgumentNullException("collection");
        }

        if (beanDefinition == null)
        {
            throw new ArgumentNullException("beanDefinition");
        }

        var collectionStore = new PostgresParameterBeanCollection<T>(ConnectionPool, collection, true);
        return collectionStore.ExecuteInsert(commandName, DataSourceName);
    }

    /// <inheritdoc />
    protected override IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector)
    {
        var sql = BuildUpdateQuery(beanDefinition, primaryKey, columnSelector);
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, sql);
        command.CommandTimeout = 0;
        AddUpdateParameters(bean, beanDefinition, command.Parameters, columnSelector);
        return command.ExecuteReader();
    }

    /// <summary>
    /// Crée la requête SQL d'insertion.
    /// </summary>
    /// <param name="beanDefinition">Définition du bean.</param>
    /// <param name="dbGeneratedPK">True si la clef est générée par la base.</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
    /// <returns>Requête SQL.</returns>
    private string BuildInsertQuery(BeanDefinition beanDefinition, bool dbGeneratedPK, ColumnSelector columnSelector)
    {
        var sbInsert = new StringBuilder(CurrentUserStatementLog);
        sbInsert.Append("insert into ");
        sbInsert.Append(beanDefinition.ContractName).Append("(");
        var sbValues = new StringBuilder(") values (");
        var count = 0;
        BeanPropertyDescriptor primaryKey = null;
        foreach (var property in beanDefinition.Properties)
        {
            if (property.IsPrimaryKey && primaryKey == null)
            {
                primaryKey = property;
                continue;
            }

            if (dbGeneratedPK && (
                property.MemberName == null
                || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName)))
            {
                continue;
            }

            if (count > 0)
            {
                sbInsert.Append(", ");
                sbValues.Append(", ");
            }

            sbInsert.Append(property.MemberName);

            sbValues.Append(VariablePrefix);
            sbValues.Append(property.MemberName);
            count++;
        }

        sbInsert.Append(sbValues.ToString()).Append(")\n");
        if (dbGeneratedPK)
        {
            sbInsert.Append($"returning {primaryKey.MemberName}");
        }

        return sbInsert.ToString();
    }
}
