using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.Common;

public abstract partial class SqlParameterBeanCollection<T>
    where T : class, new()
{
    /// <summary>
    /// Execute l'insertion en base de la collection.
    /// </summary>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="dataSourceName">Nom de la dataSource.</param>
    /// <returns>Liste d'objet insérés.</returns>
    public ICollection<T> ExecuteInsert(string commandName, string dataSourceName)
    {
        if (_connectionPool != null && SbInsert != null)
        {
            var command = _connectionPool.GetSqlCommand(dataSourceName, commandName, SbInsert.ToString());
            CreateParameter(command);
            command.CommandTimeout = 0;
            var primaryKey = BeanDefinition.PrimaryKey;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var source = Index![reader.GetInt32(1)!.Value];
                primaryKey.SetValue(source, reader.GetInt32(0)!.Value);
            }
        }

        return Collection;
    }
}
