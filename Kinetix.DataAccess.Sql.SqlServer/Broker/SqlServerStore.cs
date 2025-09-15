using System.Text;
using Kinetix.DataAccess.Sql.Broker;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.DataAccess.Sql.Common.Broker;
using Kinetix.Modeling;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer.Broker;

/// <summary>
/// Store de base pour le stockage en base de données.
/// </summary>
/// <typeparam name="T">Type du store.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="dataSourceName">Nom de la chaine de base de données.</param>
/// <param name="connectionPool">Pool de connexions.</param>
/// <param name="logger">Logger.</param>
internal class SqlServerStore<T>(string dataSourceName, ConnectionPool connectionPool, ILogger<BrokerManager> logger)
    : SqlStore<T>(dataSourceName, connectionPool, logger)
    where T : class, new()
{
    /// <inheritdoc />
    protected override string VariablePrefix => "@";

    /// <inheritdoc />
    protected override string ConcatCharacter => " + ";

    /// <inheritdoc />
    protected override string BuildInsertQuery(
        BeanDefinition beanDefinition,
        bool isGeneratedPK,
        ColumnSelector columnSelector
    )
    {
        var sbInsert = new StringBuilder(CurrentUserStatementLog);
        sbInsert.Append("insert into ");
        sbInsert.Append(beanDefinition.ContractName).Append('(');
        var sbValues = new StringBuilder(") values (");
        var count = 0;

        foreach (var property in beanDefinition.Properties)
        {
            if (property == beanDefinition.PrimaryKey && isGeneratedPK)
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
        if (isGeneratedPK)
        {
            sbInsert.Append("select cast(SCOPE_IDENTITY() as int)");
        }

        return sbInsert.ToString();
    }

    /// <inheritdoc />
    protected override ICollection<T> InsertAll(
        string commandName,
        ICollection<T> collection,
        BeanDefinition beanDefinition
    )
    {
        ArgumentNullException.ThrowIfNull(collection);

        ArgumentNullException.ThrowIfNull(beanDefinition);

        var collectionStore = new SqlServerParameterBeanCollection<T>(ConnectionPool, collection, isInsert: true);
        return collectionStore.ExecuteInsert(commandName, DataSourceName);
    }
}
