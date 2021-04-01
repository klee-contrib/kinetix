﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Kinetix.Broker
{
    /// <summary>
    /// Store de base pour le stockage en base de données.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public abstract class SqlStore<T> : IStore<T>
        where T : class, new()
    {
        /// <summary>
        /// Préfixe générique d'un service de sélection.
        /// </summary>
        private const string ServiceSelect = "SV_SELECT";

        /// <summary>
        /// Préfixe générique d'un service d'insertion.
        /// </summary>
        private const string ServiceInsert = "SV_INSERT";

        /// <summary>
        /// Préfixe générique d'un service de mise à jour.
        /// </summary>
        private const string ServiceUpdate = "SV_UPDATE";

        /// <summary>
        /// Préfixe générique d'un service de suppression.
        /// </summary>
        private const string ServiceDelete = "SV_DELETE";

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
        /// <param name="logger">Logger.</param>
        protected SqlStore(string dataSourceName, ILogger<BrokerManager> logger)
        {
            try
            {
                // Charge la définition de l'objet donné T.
                Definition = BeanDescriptor.GetDefinition(typeof(T), true);

                var attrs = typeof(T).GetCustomAttributes(typeof(TableAttribute), true);
                if (attrs == null || attrs.Length == 0)
                {
                    throw new NotSupportedException(typeof(T).FullName + " has no TableAttribute. Check type persistence.");
                }

                if (string.IsNullOrEmpty(Definition.ContractName))
                {
                    throw new NotSupportedException(typeof(T) + " has no ContractName defined. Check type persistence.");
                }

                if (Definition.PrimaryKey == null)
                {
                    throw new NotSupportedException(typeof(T) + " has no primary key defined.");
                }

                DataSourceName = dataSourceName ?? throw new ArgumentNullException("dataSourceName");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Echec d'instanciation du store.");
                throw new BrokerException("Broker<" + typeof(T).FullName + "> " + e.Message, e);
            }
        }

        /// <summary>
        /// Current user logging statement.
        /// </summary>
        protected string CurrentUserStatementLog
        {
            get;
            set;
        }

        /// <summary>
        /// Source de données du store.
        /// </summary>
        protected string DataSourceName { get; }

        /// <summary>
        /// Retourne la définition.
        /// </summary>
        protected BeanDefinition Definition { get; }

        /// <summary>
        /// Lancemement d'une exception si la requête retourne un nombre de lignes supérieur au maximum spécifié.
        /// </summary>
        protected virtual bool ThrowExceptionOnRowOverflow => true;

        /// <summary>
        /// Préfixe utilisé par le store pour faire référence à une variable.
        /// </summary>
        protected abstract string VariablePrefix
        {
            get;
        }

        /// <summary>
        /// Charactere de conacténation.
        /// </summary>
        protected abstract string ConcatCharacter
        {
            get;
        }

        /// <summary>
        /// Checks if the object is used by another object in the application.
        /// </summary>
        /// <param name="primaryKey">Id of the object.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if the object is used by another object.</returns>
        public virtual bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if at least one of the objects is used by another object in the application.
        /// </summary>
        /// <param name="primaryKeys">Ids of the objects.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if one of the objects is used by another object.</returns>
        public virtual bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Charge un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        /// <returns>Bean.</returns>
        public T Load(object primaryKey)
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            Definition.PrimaryKey.CheckValueType(primaryKey);

            var commandName = ServiceSelect + "_" + Definition.ContractName;

            // On charge l'objet à partir d'un seul critère
            // correspondant à sa clé primaire
            var criteria = new FilterCriteria(Definition.PrimaryKey.MemberName, Expression.Equals, primaryKey);

            var cmd = GetCommand(commandName, Definition.ContractName, criteria, null);
            return CollectionBuilder<T>.ParseCommandForSingleObject(cmd);
        }

        /// <summary>
        /// Charge toutes les données pour un type.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public IList<T> LoadAll(QueryParameter queryParameter)
        {
            var commandName = ServiceSelect + "_ALL_" + Definition.ContractName;
            return InternalLoadAll(commandName, queryParameter, new FilterCriteria());
        }

        /// <summary>
        /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
        /// </summary>
        /// <param name="criteria">Liste des critères de correpondance.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public IList<T> LoadAllByCriteria(FilterCriteria criteria, QueryParameter queryParameter)
        {
            // Les critères ne doivent pas être vides
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var commandName = ServiceSelect + "_ALL_LIKE_" + Definition.ContractName;

            return InternalLoadAll(commandName, queryParameter, criteria);
        }

        /// <summary>
        /// Récupération d'un objet à partir de critère de recherche.
        /// </summary>
        /// <param name="destination">Bean à charger.</param>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Objet.</returns>
        public T LoadByCriteria(FilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var commandName = ServiceSelect + "_LIKE_" + Definition.ContractName;
            var cmd = GetCommand(commandName, Definition.ContractName, criteria, null);
            return CollectionBuilder<T>.ParseCommandForSingleObject(cmd);
        }

        /// <summary>
        /// Dépose un bean dans le store.
        /// </summary>
        /// <param name="bean">Bean à enregistrer.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Clef primaire.</returns>
        /// <exception cref="BrokerException">Retourne une erreur en cas de mise à jour erronée.</exception>
        public object Put(T bean, ColumnSelector columnSelector = null)
        {
            if (bean == null)
            {
                throw new ArgumentNullException("bean");
            }

            BeanDescriptor.Check(bean);

            var value = Definition.PrimaryKey.GetValue(bean);

            if (value == null && Definition.PrimaryKey.PropertyType == typeof(Guid?))
            {
                value = Guid.NewGuid();
            }

            using var reader = ExecutePutReader(bean, Definition, Definition.PrimaryKey, value, columnSelector);

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
            return reader.GetValue(0);
        }

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <returns>Beans enregistrés.</returns>
        public ICollection<T> PutAll(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (collection.Count == 0)
            {
                return collection;
            }

            var commandName = ServiceInsert + "_" + Definition.ContractName;
            return InsertAll(commandName, collection, Definition);
        }

        /// <summary>
        /// Supprime un bean du store.
        /// </summary>
        /// <param name="primaryKey">Clef primaire du bean à supprimer.</param>
        public void Remove(object primaryKey)
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            Definition.PrimaryKey.CheckValueType(primaryKey);
            var commandName = ServiceDelete + "_" + Definition.ContractName;

            // On charge l'objet à partir d'un seul critère
            // correspondant à sa clé primaire
            var criteria = new FilterCriteria(Definition.PrimaryKey.MemberName, Expression.Equals, primaryKey);

            var rowsAffected = DeleteAllByCriteria(commandName, Definition.ContractName, criteria);
            if (rowsAffected == 0)
            {
                throw new BrokerException("Zero row deleted");
            }

            if (rowsAffected > 1)
            {
                throw new BrokerException("Too many rows deleted");
            }
        }

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="criteria">Critères de suppression.</param>
        public void RemoveAllByCriteria(FilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var commandName = ServiceDelete + "_ALL_LIKE_" + Definition.ContractName;
            DeleteAllByCriteria(commandName, Definition.ContractName, criteria);
        }

        /// <summary>
        /// Crée la commande.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Liste des critères de recherche.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <param name="queryParameter">Paramètre de tri des résultats et de limit des résultats.</param>
        /// <returns>IReadCommand contenant la commande.</returns>
        protected abstract IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, QueryParameter queryParameter);

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Bean inséré.</returns>
        protected abstract IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector);

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
        protected abstract IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue);

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="commandName">Nom du service.</param>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <param name="beanDefinition">Définition.</param>
        /// <returns>Beans enregistrés.</returns>
        protected abstract ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition);

        /// <summary>
        /// Met à jour un bean.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Bean mise à jour.</returns>
        protected abstract IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector);

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critères de suppression.</param>
        /// <returns>Retourne le nombre de lignes supprimées.</returns>
        protected virtual int DeleteAllByCriteria(string commandName, string tableName, FilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var command = CreateSqlCommand(commandName, CommandType.Text);
            command.CommandTimeout = 0;
            var commandText = new StringBuilder(CurrentUserStatementLog);
            commandText.Append("delete from ");
            commandText.Append(tableName);
            if (criteria.Parameters.Any())
            {
                PrepareFilterCriteria(criteria, command, commandText);
            }

            command.CommandText = commandText.ToString();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Retourne le critère sur une colonne.
        /// </summary>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>Colonne à tester.</returns>
        protected virtual string GetColumnCriteriaByColumnName(string columnName)
        {
            return columnName;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        /// <returns>Une nouvelle instance d'une classe héritant de AbstractSqlCommand.</returns>
        protected abstract SqlServerCommand CreateSqlCommand(string commandName, CommandType commandType);

        /// <summary>
        /// Ajoute les paramètres de la requête select (noms des colonnes, clauses from, where et order by).
        /// </summary>
        /// <param name="commandText">Requête SQL à laquelle seront ajoutés les paramètres.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="sortOrder">Ordre de tri.</param>
        /// <param name="command">Commande d'appel à la base de données.</param>
        protected void AppendSelectParameters(StringBuilder commandText, string tableName, FilterCriteria criteria, string sortOrder, SqlServerCommand command)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("commandText");
            }

            var properties = BeanDescriptor.GetDefinition(typeof(T)).Properties;
            var hasColumn = false;
            foreach (var property in properties)
            {
                if (string.IsNullOrEmpty(property.MemberName))
                {
                    continue;
                }

                if (property.PropertyType == typeof(byte[]))
                {
                    continue;
                }

                if (hasColumn)
                {
                    commandText.Append(", ");
                }

                commandText.Append(property.MemberName);
                hasColumn = true;
            }

            commandText.Append(" from ").Append(tableName);

            PrepareFilterCriteria(criteria, command, commandText);

            // Ajout du Order By si non-nul
            if (!string.IsNullOrEmpty(sortOrder))
            {
                commandText.Append(" order by ");
                commandText.Append(sortOrder);
            }
        }

        /// <summary>
        /// Prépare la chaîne SQL et les paramètres de commandes pour appliquer un FilterCriteria.
        /// </summary>
        /// <param name="filter">Critères de filtrage.</param>
        /// <param name="command">Commande.</param>
        /// <param name="commandText">Texte de la commande.</param>
        protected void PrepareFilterCriteria(FilterCriteria filter, SqlServerCommand command, StringBuilder commandText)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandText == null)
            {
                throw new ArgumentNullException("commandText");
            }

            var pos = 0;
            var mapParameters = new Dictionary<string, int>();
            foreach (var criteriaParam in filter.Parameters)
            {
                commandText.Append(pos == 0 ? " where " : " and ");
                commandText.Append(GetColumnCriteriaByColumnName(criteriaParam.ColumnName));

                string parameterName = null;
                if (!mapParameters.ContainsKey(criteriaParam.ColumnName))
                {
                    parameterName = criteriaParam.ColumnName;
                    mapParameters.Add(criteriaParam.ColumnName, 1);
                }
                else
                {
                    mapParameters[criteriaParam.ColumnName] = mapParameters[criteriaParam.ColumnName] + 1;
                    parameterName = criteriaParam.ColumnName + mapParameters[criteriaParam.ColumnName].ToString(CultureInfo.InvariantCulture);
                }

                if (criteriaParam.Expression == Expression.Between)
                {
                    var dateValues = (DateTime[])criteriaParam.Value;
                    command.Parameters.AddWithValue(parameterName + "T1", dateValues[0]);
                    command.Parameters.AddWithValue(parameterName + "T2", dateValues[1]);
                }
                else
                {
                    command.Parameters.AddWithValue(parameterName, criteriaParam.Value);
                }

                commandText.Append(GetSqlString(parameterName, criteriaParam));
                ++pos;
            }
        }

        /// <summary>
        /// Ajoute les paramètres d'insertion.
        /// </summary>
        /// <param name="bean">Bean à insérér.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="parameters">Paramètres de la commande SQL.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        protected void AddInsertParameters(T bean, BeanDefinition beanDefinition, SqlServerParameterCollection parameters, ColumnSelector columnSelector)
        {
            if (beanDefinition == null)
            {
                throw new ArgumentNullException("beanDefinition");
            }

            BeanPropertyDescriptor primaryKey = null;
            foreach (var property in beanDefinition.Properties)
            {
                if (property.IsPrimaryKey && primaryKey == null)
                {
                    primaryKey = property;
                    continue;
                }

                if (property.MemberName == null || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName))
                {
                    continue;
                }

                var value = property.GetValue(bean);

                // Ajout du paramètre en entrée de la commande.
                var parameter = AddParameter(parameters, property, value);
                if (property.PrimitiveType == typeof(byte[]))
                {
                    parameter.DbType = DbType.Binary;
                }
            }
        }

        /// <summary>
        /// Ajoute les paramètres à une commande de mise à jour.
        /// </summary>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="parameters">Paramètres de la commande SQL.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        protected void AddUpdateParameters(T bean, BeanDefinition beanDefinition, SqlServerParameterCollection parameters, ColumnSelector columnSelector)
        {
            if (beanDefinition == null)
            {
                throw new ArgumentNullException("beanDefinition");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            foreach (var property in beanDefinition.Properties)
            {
                if (property.MemberName == null || columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName) && !property.IsPrimaryKey)
                {
                    continue;
                }

                var value = property.GetValue(bean);

                if (property.IsPrimaryKey)
                {
                    AddPrimaryKeyParameter(parameters, property.MemberName, value);
                }

                // Ajout du paramètre en entrée de la commande envoyée à SQL Server.
                var parameter = AddParameter(parameters, property, value);
                if (property.PrimitiveType == typeof(byte[]))
                {
                    parameter.DbType = DbType.Binary;
                }
            }
        }

        /// <summary>
        /// Ajout du paramètre en entrée de la commande envoyée à SQL Server.
        /// </summary>
        /// <param name="parameters">Collection des paramètres dans laquelle ajouter le nouveau paramètre.</param>
        /// <param name="primaryKeyName">Nom de la clé primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clé primaire.</param>
        protected abstract void AddPrimaryKeyParameter(SqlServerParameterCollection parameters, string primaryKeyName, object primaryKeyValue);

        /// <summary>
        /// Crée la requête SQL de mise à jour d'un bean.
        /// </summary>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Requête SQL.</returns>
        protected string BuildUpdateQuery(BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector)
        {
            if (beanDefinition == null)
            {
                throw new ArgumentNullException("beanDefinition");
            }

            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            var sbUpdate = new StringBuilder(CurrentUserStatementLog);
            sbUpdate.Append("update ");

            var sbUpdateSet = new StringBuilder(beanDefinition.ContractName);
            sbUpdateSet.Append(" set");

            var sbUpdateWhere = new StringBuilder(" where ");
            sbUpdateWhere.Append(primaryKey.MemberName).Append(" = ").Append(VariablePrefix).Append(primaryKey.MemberName);

            // Construction des champs de l'update SET et du WHERE
            var count = 0;
            foreach (var property in beanDefinition.Properties)
            {
                // Si la propriété est une clé primaire ou n'est pas défini,
                // on passe à la propriété suivante.
                if (property.MemberName == null || property.IsPrimaryKey || property.IsReadOnly ||
                        columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName))
                {
                    continue;
                }

                BuildUpdateSet(sbUpdateSet, count, property);
                count++;
            }

            sbUpdate.Append(sbUpdateSet).Append(sbUpdateWhere);
            return sbUpdate.ToString();
        }

        /// <summary>
        /// Crée la chaine lié au set.
        /// </summary>
        /// <param name="sbUpdateSet">Clause Set.</param>
        /// <param name="count">Index courant.</param>
        /// <param name="property">Propriété courante.</param>
        protected void BuildUpdateSet(StringBuilder sbUpdateSet, int count, BeanPropertyDescriptor property)
        {
            if (sbUpdateSet == null)
            {
                throw new ArgumentNullException("sbUpdateSet");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (count > 0)
            {
                sbUpdateSet.Append(",");
            }

            sbUpdateSet.Append(" ").Append(property.MemberName).Append(" = ");

            // Insertion de la valeur à mettre à jour
            sbUpdateSet.Append(VariablePrefix).Append(property.MemberName);
        }

        /// <summary>
        /// Ajoute un paramètre à une collection avec sa valeur.
        /// </summary>
        /// <param name="parameters">Collection de paramètres dans laquelle le nouveau paramètre est créé.</param>
        /// <param name="property">Propriété correspondant au paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        protected abstract SqlServerParameter AddParameter(SqlServerParameterCollection parameters, BeanPropertyDescriptor property, object value);

        /// <summary>
        /// Retourne la traduction SQL du paramètre de filtrage considéré.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="criteriaParam">Le parametre considéré.</param>
        /// <returns>L'expression traduite en SQL.</returns>
        private string GetSqlString(string parameterName, FilterCriteriaParam criteriaParam)
        {
            return criteriaParam.Expression switch
            {
                Expression.Between => " BETWEEN " + VariablePrefix + parameterName + "T1" + " AND " + VariablePrefix + parameterName + "T2",
                Expression.Contains => " LIKE '%' + " + VariablePrefix + parameterName + " " + ConcatCharacter + " '%'",
                Expression.EndsWith => " LIKE '%' + " + VariablePrefix + parameterName,
                Expression.Equals => " = " + VariablePrefix + parameterName,
                Expression.GreaterOrEquals => " >= " + VariablePrefix + parameterName,
                Expression.LowerOrEquals => " <= " + VariablePrefix + parameterName,
                Expression.Greater => " > " + VariablePrefix + parameterName,
                Expression.IsNotNull => " IS NOT NULL",
                Expression.IsNull => " IS NULL",
                Expression.Lower => " < " + VariablePrefix + parameterName,
                Expression.NotStartsWith => " NOT LIKE " + VariablePrefix + parameterName + " " + ConcatCharacter + "'%'",
                Expression.StartsWith => " LIKE " + VariablePrefix + parameterName + " " + ConcatCharacter + " '%'",
                Expression.NotEquals => " != " + VariablePrefix + parameterName,
                _ => throw new NotSupportedException("Type d'expression de filtre non supportée : " + criteriaParam.Expression.ToString()),
            };
        }

        /// <summary>
        /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <param name="criteria">Map de critères auquelle la recherche doit correpondre.</param>
        /// <returns>Collection.</returns>
        private IList<T> InternalLoadAll(string commandName, QueryParameter queryParameter, FilterCriteria criteria)
        {
            if (queryParameter != null)
            {
                // Définition du tri à partir de la requete.
                queryParameter.RemapSortColumn(typeof(T));
            }

            var cmd = GetCommand(commandName, Definition.ContractName, criteria, queryParameter);
            return CollectionBuilder<T>.ParseCommand(cmd).ToList();
        }

        /// <summary>
        /// Obtient un reader du résultat d'enregistrement.
        /// </summary>
        /// <param name="bean">Bean à saugevarder.</param>
        /// <param name="beanDefinition">Definition.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>DataReader contenant le bean sauvegardé.</returns>
        private IDataReader ExecutePutReader(T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector)
        {
            if (primaryKey.GetValue(bean) != null)
            {
                var commandName = ServiceUpdate + "_" + beanDefinition.ContractName;
                return Update(commandName, bean, beanDefinition, primaryKey, primaryKeyValue, columnSelector);
            }

            if (primaryKeyValue == null)
            {
                var commandName = ServiceInsert + "_" + beanDefinition.ContractName;
                return Insert(commandName, bean, beanDefinition, primaryKey, columnSelector);
            }
            else
            {
                var commandName = ServiceInsert + "_" + beanDefinition.ContractName;
                return Insert(commandName, bean, beanDefinition, primaryKey, columnSelector, primaryKeyValue);
            }
        }
    }
}
