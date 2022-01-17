using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Kinetix.ComponentModel;
using Microsoft.SqlServer.Server;

namespace Kinetix.DataAccess.Sql.SqlServer
{
    /// <summary>
    /// Contient les informations nécéssaires à l'insertion et la mise à jour ensembliste des données.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    internal class SqlServerParameterBeanCollection<T> : SqlParameterBeanCollection<T>
        where T : class, new()
    {
        private readonly List<SqlMetaData> _metadataList = new();

        private List<SqlDataRecord> _dataRecordList;
        private BeanPropertyDescriptor _insertKeyProp;
        private string _typeName;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="connectionPool">Pool de connexion.</param>
        /// <param name="collection">Collection d'objet.</param>
        /// <param name="isInsert">True si les parmètres sont utilisés pour une insertion.</param>
        public SqlServerParameterBeanCollection(ConnectionPool connectionPool, ICollection<T> collection, bool isInsert)
            : base(connectionPool, collection, isInsert)
        {
        }

        /// <inheritdoc />
        protected override void Init()
        {
            _insertKeyProp = BeanDefinition.Properties["InsertKey"];
            _typeName = BeanDefinition.ContractName + "_TABLE_TYPE";

            SbInsert = new StringBuilder("insert into ");
            SbInsert.Append(BeanDefinition.ContractName).Append("(");

            var sbOutput = new StringBuilder(") output ");
            var sbSelect = new StringBuilder(" select ");

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

                    sbOutput.Append("INSERTED.").Append(property.MemberName);

                    outputCount++;
                }
            }

            SbInsert.Append(", ").Append(_insertKeyProp.MemberName);
            sbSelect.Append(", ").Append(_insertKeyProp.MemberName);
            sbOutput.Append(", INSERTED.").Append(_insertKeyProp.MemberName);
            _metadataList.Add(new SqlMetaData(_insertKeyProp.MemberName, SqlDbType.Int));

            SbInsert.Append(sbOutput).Append(sbSelect).Append(" from @table");
        }

        /// <inheritdoc />
        protected override void PopulateParamList(bool isInsert)
        {
            Index = new Dictionary<int, T>();
            var array = _metadataList.ToArray();
            _dataRecordList = new List<SqlDataRecord>();
            var insertKey = 0;
            foreach (var item in Collection)
            {
                if (isInsert && BeanDefinition.PrimaryKey.GetValue(item) != null)
                {
                    throw new NotSupportedException("La clef primaire doit être nulle.");
                }

                var record = new SqlDataRecord(array);

                var ordinal = 0;
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

                    record.SetValue(ordinal, value);
                    ++ordinal;
                }

                record.SetValue(ordinal, insertKey);
                _dataRecordList.Add(record);
                Index.Add(insertKey, item);
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

        /// <inheritdoc />
        protected override SqlDataParameter PopulateSqlDataParameter(SqlDataParameter parameter)
        {
            parameter.ParameterName = "@table";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = _dataRecordList;
            ((SqlParameter)parameter.InnerParameter).SqlDbType = SqlDbType.Structured;
            ((SqlParameter)parameter.InnerParameter).TypeName = _typeName;
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
}