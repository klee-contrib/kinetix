using System.Data;
using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.Broker;

public partial class SqlStore<T>
{
    /// <inheritdoc cref="IStore{T}.Load" />
    public T? Load(object primaryKey)
    {
        var criteria = CreatePrimaryKeyCriteria(primaryKey);
        var commandName = ServiceSelect + "_" + Definition.ContractName;

        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        ConfigureCommand(command, Definition.ContractName!, criteria, queryParameter: null);
        return CollectionBuilder<T>.ParseCommandForSingleObject(command);
    }

    /// <inheritdoc cref="IStore{T}.LoadAll" />
    public IList<T> LoadAll(QueryParameter? queryParameter)
    {
        var commandName = ServiceSelect + "_ALL_" + Definition.ContractName;
        return InternalLoadAll(commandName, queryParameter, new FilterCriteria());
    }

    /// <inheritdoc cref="IStore{T}.LoadAllByCriteria" />
    public IList<T> LoadAllByCriteria(FilterCriteria criteria, QueryParameter? queryParameter)
    {
        // Les critères ne doivent pas être vides
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceSelect + "_ALL_LIKE_" + Definition.ContractName;

        return InternalLoadAll(commandName, queryParameter, criteria);
    }

    /// <inheritdoc cref="IStore{T}.LoadByCriteria" />
    public T? LoadByCriteria(FilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceSelect + "_LIKE_" + Definition.ContractName;
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        ConfigureCommand(command, Definition.ContractName!, criteria, queryParameter: null);
        return CollectionBuilder<T>.ParseCommandForSingleObject(command);
    }

    /// <inheritdoc cref="IStore{T}.Put" />
    public object Put(T bean, bool forceInsert, ColumnSelector? columnSelector = null)
    {
        var (value, commandName) = PreparePut(bean, forceInsert, columnSelector);
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        ConfigurePutCommand(command, bean, value, forceInsert, columnSelector);
        using var reader = command.ExecuteReader();

        ValidatePutResult(reader.RecordsAffected);

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

    /// <inheritdoc cref="IStore{T}.PutAll" />
    public ICollection<T> PutAll(ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0)
        {
            return collection;
        }

        var commandName = ServiceInsert + "_" + Definition.ContractName;
        return InsertAll(commandName, collection, Definition);
    }

    /// <inheritdoc cref="IStore{T}.Remove" />
    public void Remove(object primaryKey)
    {
        var criteria = CreatePrimaryKeyCriteria(primaryKey);
        var commandName = ServiceDelete + "_" + Definition.ContractName;

        var rowsAffected = InternalDeleteAllByCriteria(commandName, Definition.ContractName!, criteria);
        ValidateDeleteResult(rowsAffected);
    }

    /// <inheritdoc cref="IStore{T}.RemoveAllByCriteria" />
    public void RemoveAllByCriteria(FilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var commandName = ServiceDelete + "_ALL_LIKE_" + Definition.ContractName;
        InternalDeleteAllByCriteria(commandName, Definition.ContractName!, criteria);
    }

    /// <summary>
    /// <param name="commandName">Nom du service.</param>
    /// <param name="collection">Beans à enregistrer.</param>
    /// <param name="beanDefinition">Définition.</param>
    /// </summary>
    /// <returns>Beans enregistrés.</returns>
    protected abstract ICollection<T> InsertAll(
        string commandName,
        ICollection<T> collection,
        BeanDefinition beanDefinition
    );

    private int InternalDeleteAllByCriteria(string commandName, string tableName, FilterCriteria criteria)
    {
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        ConfigureDeleteCommand(command, tableName, criteria);
        return command.ExecuteNonQuery();
    }

    private List<T> InternalLoadAll(string commandName, QueryParameter? queryParameter, FilterCriteria criteria)
    {
        var command = ConnectionPool.GetSqlCommand(DataSourceName, commandName, CommandType.Text);
        ConfigureLoadAllCommand(command, queryParameter, criteria);
        return CollectionBuilder<T>.ParseCommand(command).ToList();
    }
}
