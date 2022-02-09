using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Kinetix.Broker
{
    /// <summary>
    /// Store SqlServer.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class SqlServerStore<T> : SqlStore<T>
        where T : class, new()
    {
        private readonly ConnectionPool _connectionPool;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
        /// <param name="connectionPool">Pool de connexions.</param>
        /// <param name="logger">Logger.</param>
        public SqlServerStore(string dataSourceName, ConnectionPool connectionPool, ILogger<BrokerManager> logger)
            : base(dataSourceName, logger)
        {
            _connectionPool = connectionPool;
        }

        /// <summary>
        /// Préfixe utilisé par le store pour faire référence à une variable.
        /// </summary>
        protected override string VariablePrefix => "@";

        /// <summary>
        /// Charactere de conacténation.
        /// </summary>
        protected override string ConcatCharacter => " + ";

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérér.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Reader retournant les données du bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector)
        {
            var sql = BuildInsertQuery(beanDefinition, true, columnSelector);
            var command = _connectionPool.GetSqlServerCommand(DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <returns>Bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue)
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            var sql = BuildInsertQuery(beanDefinition, true, columnSelector);
            var command = _connectionPool.GetSqlServerCommand(DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            command.Parameters.AddWithValue(primaryKey.MemberName, primaryKeyValue);
            AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="commandName">Nom du service.</param>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <param name="beanDefinition">Définition.</param>
        /// <returns>Beans enregistrés.</returns>
        protected override ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (beanDefinition == null)
            {
                throw new ArgumentNullException("beanDefinition");
            }

            var collectionStore = new SqlServerParameterBeanCollection<T>(_connectionPool, collection, true); ;
            return collectionStore.ExecuteInsert(commandName, DataSourceName);
        }

        /// <summary>
        /// Met à jour un enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Reader retournant les données du bean mise à jour.</returns>
        protected override IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector)
        {
            var sql = BuildUpdateQuery(beanDefinition, primaryKey, columnSelector);
            var command = _connectionPool.GetSqlServerCommand(DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            AddUpdateParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Execute une commande et retourne un reader.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <returns>DataReader contenant le résultat de la commande.</returns>
        protected override IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, QueryParameter queryParameter)
        {
            var command = _connectionPool.GetSqlServerCommand(DataSourceName, commandName, CommandType.Text);
            command.QueryParameters = queryParameter;

            var commandText = new StringBuilder("select ");

            string order = null;
            if (queryParameter != null && !string.IsNullOrEmpty(queryParameter.SortCondition))
            {
                order = queryParameter.SortCondition;
            }

            // Todo : brancher le tri.
            AppendSelectParameters(commandText, tableName, criteria, order, command);

            // Set de la requête
            command.CommandText = commandText.ToString();

            return command;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        /// <returns>Une nouvelle instance de SqlServerCommand.</returns>
        protected override SqlServerCommand CreateSqlCommand(string commandName, CommandType commandType)
        {
            return _connectionPool.GetSqlServerCommand(DataSourceName, commandName, commandType);
        }

        /// <summary>
        /// Ajout du paramètre en entrée de la commande envoyée à SQL Server.
        /// </summary>
        /// <param name="parameters">Collection des paramètres dans laquelle ajouter le nouveau paramètre.</param>
        /// <param name="primaryKeyName">Nom de la clé primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clé primaire.</param>
        protected override void AddPrimaryKeyParameter(SqlServerParameterCollection parameters, string primaryKeyName, object primaryKeyValue)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            parameters.AddWithValue("PK_" + primaryKeyName, primaryKeyValue);
        }

        /// <summary>
        /// Ajoute un paramètre à une collection avec sa valeur.
        /// </summary>
        /// <param name="parameters">Collection de paramètres dans laquelle le nouveau paramètre est créé.</param>
        /// <param name="property">Propriété correspondant au paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        protected override SqlServerParameter AddParameter(SqlServerParameterCollection parameters, BeanPropertyDescriptor property, object value)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            return parameters.AddWithValue(property.MemberName, value);
        }

        /// <summary>
        /// Crée la requête SQL d'insertion.
        /// </summary>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="dbGeneratedPK">True si la clef est générée par la base.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Requête SQL.</returns>
        private string BuildInsertQuery(BeanDefinition beanDefinition, bool dbGeneratedPK, ColumnSelector columnSelector)
        {
            var sbInsert = new StringBuilder(CurrentUserStatementLog);
            sbInsert.Append("insert into ");
            sbInsert.Append(beanDefinition.ContractName).Append("(");
            var sbValues = new StringBuilder(") values (");
            var count = 0;
            BeanPropertyDescriptor primaryKey = null;
            foreach (var property in beanDefinition.Properties)
            {
                if (property.IsPrimaryKey && primaryKey == null)
                {
                    primaryKey = property;
                    continue;
                }

                if (dbGeneratedPK && (
                    property.MemberName == null
                    || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName)))
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

            sbInsert.Append(sbValues.ToString()).Append(")\n");
            if (dbGeneratedPK)
            {
                sbInsert.Append("select cast(SCOPE_IDENTITY() as int)");
            }

            return sbInsert.ToString();
        }
    }
}