using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using Kinetix.Modeling;
using Npgsql;
using NpgsqlTypes;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Contient les informations nécéssaires à l'insertion et la mise à jour ensembliste des données.
/// </summary>
/// <typeparam name="T">Type du store.</typeparam>
internal class PostgresParameterBeanCollection<T> : SqlParameterBeanCollection<T>
    where T : class, new()
{
    private BeanPropertyDescriptor _insertKeyProp;
    private JsonArray _dataRecordList;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connectionPool">Pool de connexion.</param>
    /// <param name="collection">Collection d'objet.</param>
    /// <param name="isInsert">True si les parmètres sont utilisés pour une insertion.</param>
    public PostgresParameterBeanCollection(ConnectionPool connectionPool, ICollection<T> collection, bool isInsert)
        : base(connectionPool, collection, isInsert)
    {
    }

    /// <inheritdoc />
    protected override void Init()
    {
        _insertKeyProp = BeanDefinition.Properties["InsertKey"];

        SbInsert = new StringBuilder($"insert into {BeanDefinition.ContractName} (");

        var sbOutput = new StringBuilder(" returning ");
        var sbSelect = new StringBuilder(") select ");

        var selectCount = 0;
        var outputCount = 0;
        foreach (var property in BeanDefinition.Properties)
        {
            if (property.MemberName == null)
            {
                continue;
            }

            if (property == _insertKeyProp)
            {
                continue;
            }

            if (!(property.IsPrimaryKey && property.PrimitiveType == typeof(int)))
            {
                if (selectCount > 0)
                {
                    SbInsert.Append(", ");
                    sbSelect.Append(", ");
                }

                SbInsert.Append(property.MemberName);
                sbSelect.Append(property.MemberName);

                selectCount++;
            }
            else
            {
                if (outputCount > 0)
                {
                    sbOutput.Append(", ");
                }

                sbOutput.Append(property.MemberName);

                outputCount++;
            }
        }

        SbInsert.Append(", ").Append(_insertKeyProp.MemberName);
        sbSelect.Append(", ").Append(_insertKeyProp.MemberName);
        sbOutput.Append(", ").Append(_insertKeyProp.MemberName);

        SbInsert.Append(sbSelect).Append($" from json_populate_recordset(null::{BeanDefinition.ContractName}, @table)").Append(sbOutput);
    }

    /// <inheritdoc />
    protected override void PopulateParamList(bool isInsert)
    {
        Index = new Dictionary<int, T>();
        _dataRecordList = new JsonArray();
        var insertKey = 0;
        foreach (var item in Collection)
        {
            var record = new JsonObject();

            if (isInsert && BeanDefinition.PrimaryKey.GetValue(item) != null)
            {
                throw new NotSupportedException("La clef primaire doit être nulle.");
            }

            foreach (var property in BeanDefinition.Properties)
            {
                var value = GetPropertyValue(item, property);

                if (property.MemberName == null || property.IsPrimaryKey || property == _insertKeyProp)
                {
                    if (!isInsert && property.IsPrimaryKey && property.PrimitiveType == typeof(int))
                    {
                        insertKey = (int)(value ?? insertKey);
                    }

                    if (property.MemberName == null || property.PrimitiveType == typeof(int))
                    {
                        continue;
                    }
                }

                record.Add(property.MemberName.ToLowerInvariant(), JsonValue.Create(value));
            }

            record.Add(_insertKeyProp.MemberName.ToLowerInvariant(), JsonValue.Create(insertKey));
            _dataRecordList.Add(record);
            Index.Add(insertKey, item);
            ++insertKey;
        }
    }

    /// <inheritdoc />
    protected override SqlDataParameter PopulateSqlDataParameter(SqlDataParameter parameter)
    {
        parameter.ParameterName = "@table";
        parameter.Direction = ParameterDirection.Input;
        ((NpgsqlParameter)parameter.InnerParameter).NpgsqlDbType = NpgsqlDbType.Json;
        parameter.Value = _dataRecordList.ToJsonString();
        return parameter;
    }

    /// <summary>
    /// Renvoie la valeur de la propriété.
    /// </summary>
    /// <param name="item">Item.</param>
    /// <param name="property">Descrition de l'item.</param>
    /// <returns>Valeur.</returns>
    private static object GetPropertyValue(T item, BeanPropertyDescriptor property)
    {
        return property.GetValue(item);
    }
}
