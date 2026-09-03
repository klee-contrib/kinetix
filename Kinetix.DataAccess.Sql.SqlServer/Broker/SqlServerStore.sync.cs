using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.SqlServer.Broker;

internal partial class SqlServerStore<T>
{
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
