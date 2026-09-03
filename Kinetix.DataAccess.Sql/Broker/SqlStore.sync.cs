using System.Data;
using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.Broker;

public partial class SqlStore<T>
{
    /// <inheritdoc cref="IStore{T}.Load" />
    public T? Load(object primaryKey)
    {
        return CollectionBuilder<T>.ParseCommandForSingleObject(GetLoadCommand(primaryKey));
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
        var cmd = GetCommand(commandName, Definition.ContractName!, criteria, queryParameter: null);
        return CollectionBuilder<T>.ParseCommandForSingleObject(cmd);
    }

    /// <inheritdoc cref="IStore{T}.Put" />
    public object Put(T bean, bool forceInsert, ColumnSelector? columnSelector = null)
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

        using var reader = GetPutCommand(bean, value, forceInsert, columnSelector).ExecuteReader();

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
        ArgumentNullException.ThrowIfNull(primaryKey);

        Definition.PrimaryKey.CheckValueType(primaryKey);
        var commandName = ServiceDelete + "_" + Definition.ContractName;

        // On charge l'objet à partir d'un seul critère
        // correspondant à sa clé primaire
        var criteria = new FilterCriteria(Definition.PrimaryKey.MemberName!, Expression.Equals, primaryKey);

        var rowsAffected = InternalDeleteAllByCriteria(commandName, Definition.ContractName!, criteria);
        if (rowsAffected == 0)
        {
            throw new BrokerException("Zero row deleted");
        }

        if (rowsAffected > 1)
        {
            throw new BrokerException("Too many rows deleted");
        }
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
        return GetDeleteCommand(commandName, tableName, criteria).ExecuteNonQuery();
    }

    private List<T> InternalLoadAll(string commandName, QueryParameter? queryParameter, FilterCriteria criteria)
    {
        return CollectionBuilder<T>.ParseCommand(GetLoadAllCommand(commandName, queryParameter, criteria)).ToList();
    }
}
