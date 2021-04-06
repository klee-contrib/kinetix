using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Kinetix.ComponentModel;
using Microsoft.SqlServer.Server;

namespace Kinetix.Data.SqlClient
{
    /// <summary>
    /// Contient les informations nécéssaires à l'insertion et la mise à jour ensembliste des données.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class SqlServerParameterBeanCollection<T>
        where T : class, new()
    {
        private readonly BeanDefinition _beanDefinition;
        private readonly BeanPropertyDescriptor _insertKeyProp;
        private readonly ConnectionPool _connectionPool;
        private readonly ICollection<T> _collection;
        private readonly List<SqlMetaData> _metadataList;
        private string _typeName;
        private StringBuilder _sbInsert;
        private Dictionary<int, T> _index;
        private List<SqlDataRecord> _dataRecordList;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="collection">Collection d'objet.</param>
        /// <param name="isInsert">True si les parmètres sont utilisés pour une insertion.</param>
        public SqlServerParameterBeanCollection(ConnectionPool connectionPool, ICollection<T> collection, bool isInsert)
        {
            _connectionPool = connectionPool;
            _collection = collection;
            _beanDefinition = BeanDescriptor.GetDefinition(typeof(T), true);
            _metadataList = new List<SqlMetaData>();
            _insertKeyProp = _beanDefinition.Properties["InsertKey"];
            if (_insertKeyProp == null)
            {
                throw new NotSupportedException("Le type " + _beanDefinition.BeanType + " doit définir une propriété de InsertKey.");
            }

            Init();
            PopulateParamList(isInsert);
        }

        /// <summary>
        /// Execute l'insertion en base de la collection.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="dataSourceName">Nom de la dataSource.</param>
        /// <returns>Liste d'objet insérés.</returns>
        public ICollection<T> ExecuteInsert(string commandName, string dataSourceName)
        {
            var command = _connectionPool.GetSqlServerCommand(dataSourceName, commandName, _sbInsert.ToString());
            CreateParameter(command);
            command.CommandTimeout = 0;
            var primaryKey = _beanDefinition.PrimaryKey;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var source = _index[reader.GetInt32(1).Value];
                    primaryKey.SetValue(source, reader.GetInt32(0).Value);
                }
            }

            return _collection;
        }

        /// <summary>
        /// Crée le paramètre de liste a ajouter à la commande.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter CreateParameter(IDbCommand command)
        {
            var parameter = PopulateSqlServerParameter(new SqlServerParameter(command.CreateParameter()));
            command.Parameters.Add(parameter.InnerParameter);
            return parameter;
        }

        /// <summary>
        /// Crée le paramètre de liste a ajouter à la commande.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter CreateParameter(SqlServerCommand command)
        {
            var parameter = PopulateSqlServerParameter(command.CreateParameter());
            command.Parameters.Add(parameter);
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

        /*
        /// <summary>
        /// Crée le type en base s'il n'existe pas.
        /// </summary>
        /// <param name="dataSourceName">Bdd.</param>
        private void ExecuteCreateType(string dataSourceName) {
            lock (_lock) {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Suppress)) {
                    SqlServerCommand cmd = new SqlServerCommand(dataSourceName, _beanDefinition.ContractName + "_CREATE_TYPE", _sbType.ToString(), true);
                    cmd.ExecuteNonQuery();
                    tx.Complete();
                }
            }
        }*/

        /// <summary>
        /// Initialise les données de métatdata.
        /// </summary>
        private void Init()
        {
            _typeName = _beanDefinition.ContractName + "_TABLE_TYPE";
            _sbInsert = new StringBuilder("insert into ");
            _sbInsert.Append(_beanDefinition.ContractName).Append("(");

            var sbOutput = new StringBuilder(") output ");
            var sbSelect = new StringBuilder(" select ");

            var selectCount = 0;
            var outputCount = 0;
            foreach (var property in _beanDefinition.Properties)
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
                        _sbInsert.Append(", ");
                        sbSelect.Append(", ");
                    }

                    SqlMetaData metaData;
                    if (property.PrimitiveType == typeof(int))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.Int);
                    }
                    else if (property.PrimitiveType == typeof(short))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.SmallInt);
                    }
                    else if (property.PrimitiveType == typeof(decimal))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.Decimal, 19, 9);
                    }
                    else if (property.PrimitiveType == typeof(string))
                    {
                        metaData = property.Domain.Length.HasValue
                            ? new SqlMetaData(property.MemberName, SqlDbType.NVarChar, property.Domain.Length.Value)
                            : new SqlMetaData(property.MemberName, SqlDbType.Text);
                    }
                    else if (property.PrimitiveType == typeof(DateTime))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.DateTime2);
                    }
                    else if (property.PrimitiveType == typeof(bool))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.Bit);
                    }
                    else if (property.PrimitiveType == typeof(byte[]))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.Image);
                    }
                    else if (property.PrimitiveType == typeof(System.Guid))
                    {
                        metaData = new SqlMetaData(property.MemberName, SqlDbType.UniqueIdentifier);
                    }
                    else
                    {
                        throw new NotSupportedException("Type non supporté : " + property.PrimitiveType + " pour " + property.MemberName);
                    }

                    _metadataList.Add(metaData);

                    _sbInsert.Append(property.MemberName);
                    sbSelect.Append(property.MemberName);

                    selectCount++;
                }
                else
                {
                    if (outputCount > 0)
                    {
                        sbOutput.Append(", ");
                    }

                    sbOutput.Append("INSERTED.").Append(property.MemberName);

                    outputCount++;
                }
            }

            _sbInsert.Append(", ").Append(_insertKeyProp.MemberName);
            sbSelect.Append(", ").Append(_insertKeyProp.MemberName);
            sbOutput.Append(", INSERTED.").Append(_insertKeyProp.MemberName);
            _metadataList.Add(new SqlMetaData(_insertKeyProp.MemberName, SqlDbType.Int));

            _sbInsert.Append(sbOutput).Append(sbSelect).Append(" from @table");
        }

        /// <summary>
        /// Rempli la liste de SQL record.
        /// </summary>
        /// <param name="isInsert">True si c'est pour une insertion.</param>
        private void PopulateParamList(bool isInsert)
        {
            _index = new Dictionary<int, T>();
            var array = _metadataList.ToArray();
            _dataRecordList = new List<SqlDataRecord>();
            var insertKey = 0;
            foreach (var item in _collection)
            {
                if (isInsert && _beanDefinition.PrimaryKey.GetValue(item) != null)
                {
                    throw new NotSupportedException("La clef primaire doit être nulle.");
                }

                var record = new SqlDataRecord(array);

                var ordinal = 0;
                foreach (var property in _beanDefinition.Properties)
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

                    record.SetValue(ordinal, value);
                    ++ordinal;
                }

                record.SetValue(ordinal, insertKey);
                _dataRecordList.Add(record);
                _index.Add(insertKey, item);
                ++insertKey;
            }

            if (_dataRecordList.Count == 0)
            {
                var record = new SqlDataRecord(array);
                for (var i = 0; i < array.Length; i++)
                {
                    record.SetValue(i, null);
                }

                _dataRecordList.Add(record);
            }
        }

        /// <summary>
        /// Renvoie le pramaetre mis à jour.
        /// </summary>
        /// <param name="parameter">Parametre.</param>
        /// <returns>Parametre mis à jour.</returns>
        private SqlServerParameter PopulateSqlServerParameter(SqlServerParameter parameter)
        {
            parameter.ParameterName = "@table";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Direction = ParameterDirection.Input;
            parameter.TypeName = _typeName;
            parameter.Value = _dataRecordList;
            return parameter;
        }
    }
}