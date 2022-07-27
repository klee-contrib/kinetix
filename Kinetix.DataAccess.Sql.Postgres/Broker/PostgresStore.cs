using System.Text;
using Kinetix.DataAccess.Sql.Broker;
using Kinetix.Modeling;
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
    protected override string BuildInsertQuery(BeanDefinition beanDefinition, bool dbGeneratedPK, ColumnSelector columnSelector)
    {
        var sbInsert = new StringBuilder(CurrentUserStatementLog);
        sbInsert.Append("insert into ");
        sbInsert.Append(beanDefinition.ContractName).Append('(');
        var sbValues = new StringBuilder(") values (");
        var count = 0;

        foreach (var property in beanDefinition.Properties)
        {
            if (property == beanDefinition.PrimaryKey && dbGeneratedPK)
            {
                continue;
            }

            if (property.MemberName == null || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName))
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

        sbInsert.Append(sbValues).Append(")\n");
        if (dbGeneratedPK)
        {
            sbInsert.Append($"returning {beanDefinition.PrimaryKey.MemberName}");
        }

        return sbInsert.ToString();
    }

    /// <inheritdoc />
    protected override ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (beanDefinition == null)
        {
            throw new ArgumentNullException(nameof(beanDefinition));
        }

        var collectionStore = new PostgresParameterBeanCollection<T>(ConnectionPool, collection, true);
        return collectionStore.ExecuteInsert(commandName, DataSourceName);
    }
}
