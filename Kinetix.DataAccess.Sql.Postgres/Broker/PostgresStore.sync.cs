using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.Postgres.Broker;

internal partial class PostgresStore<T>
{
    protected override ICollection<T> InsertAll(
        string commandName,
        ICollection<T> collection,
        BeanDefinition beanDefinition
    )
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(beanDefinition);

        var collectionStore = new PostgresParameterBeanCollection<T>(ConnectionPool, collection, isInsert: true);
        return collectionStore.ExecuteInsert(commandName, DataSourceName);
    }
}
