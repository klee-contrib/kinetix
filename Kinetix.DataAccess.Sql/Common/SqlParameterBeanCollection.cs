using System.Data;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Contient les informations nécéssaires à l'insertion et la mise à jour ensembliste des données.
/// </summary>
/// <typeparam name="T">Type du store.</typeparam>
public abstract class SqlParameterBeanCollection<T>
    where T : class, new()
{
    private readonly ConnectionPool _connectionPool;
    private readonly BeanPropertyDescriptor _insertKeyProp;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connectionPool">Pool de connexion.</param>
    /// <param name="collection">Collection d'objet.</param>
    /// <param name="isInsert">True si les parmètres sont utilisés pour une insertion.</param>
    public SqlParameterBeanCollection(ConnectionPool connectionPool, ICollection<T> collection, bool isInsert)
    {
        Collection = collection;
        _connectionPool = connectionPool;
        BeanDefinition = BeanDescriptor.GetDefinition(typeof(T), true);
        _insertKeyProp = BeanDefinition.Properties["InsertKey"];
        if (_insertKeyProp == null)
        {
            throw new NotSupportedException("Le type " + BeanDefinition.BeanType + " doit définir une propriété de InsertKey.");
        }

        Init();
        PopulateParamList(isInsert);
    }
    /// <summary>
    /// Définition du bean.
    /// </summary>
    protected BeanDefinition BeanDefinition { get; }

    /// <summary>
    /// Collection.
    /// </summary>
    protected ICollection<T> Collection { get; }

    /// <summary>
    /// Index.
    /// </summary>
    protected Dictionary<int, T> Index { get; set; }

    /// <summary>
    /// StringBuilder pour l'insert.
    /// </summary>
    protected StringBuilder SbInsert { get; set; }

    /// <summary>
    /// Crée le paramètre de liste a ajouter à la commande.
    /// </summary>
    /// <param name="command">Commande.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter CreateParameter(IDbCommand command)
    {
        var parameter = PopulateSqlDataParameter(new SqlDataParameter(command.CreateParameter()));
        command.Parameters.Add(parameter.InnerParameter);
        return parameter;
    }

    /// <summary>
    /// Crée le paramètre de liste a ajouter à la commande.
    /// </summary>
    /// <param name="command">Commande.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter CreateParameter(BaseSqlCommand command)
    {
        var parameter = PopulateSqlDataParameter(command.CreateParameter());
        command.Parameters.Add(parameter);
        return parameter;
    }

    /// <summary>
    /// Execute l'insertion en base de la collection.
    /// </summary>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="dataSourceName">Nom de la dataSource.</param>
    /// <returns>Liste d'objet insérés.</returns>
    public ICollection<T> ExecuteInsert(string commandName, string dataSourceName)
    {
        var command = _connectionPool.GetSqlCommand(dataSourceName, commandName, SbInsert.ToString());
        CreateParameter(command);
        command.CommandTimeout = 0;
        var primaryKey = BeanDefinition.PrimaryKey;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var source = Index[reader.GetInt32(1).Value];
                primaryKey.SetValue(source, reader.GetInt32(0).Value);
            }
        }

        return Collection;
    }

    /// <summary>
    /// Initialise les données de métatdata.
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// Rempli la liste de SQL record.
    /// </summary>
    /// <param name="isInsert">True si c'est pour une insertion.</param>
    protected abstract void PopulateParamList(bool isInsert);

    /// <summary>
    /// Renvoie le pramaetre mis à jour.
    /// </summary>
    /// <param name="parameter">Parametre.</param>
    /// <returns>Parametre mis à jour.</returns>
    protected abstract SqlDataParameter PopulateSqlDataParameter(SqlDataParameter parameter);
}
