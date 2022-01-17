using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace Kinetix.DataAccess.Sql.SqlServer
{
    /// <summary>
    /// Analyseur de requête SQL Dynamique.
    /// </summary>
    internal class SqlServerCommandParser : CommandParser
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="sqlManager">Composant injecté.</param>
        public SqlServerCommandParser(SqlManager sqlManager)
            : base(sqlManager)
        {
        }

        /// <inheritdoc />
        protected override bool IsNull(object parameter)
        {
            var param = (SqlParameter)parameter;
            if (param.SqlDbType != SqlDbType.Structured)
            {
                return DBNull.Value.Equals(param.Value);
            }
            else
            {
                var listValue = (IList<SqlDataRecord>)param.Value;
                return listValue.Count == 1 && listValue[0][0] == DBNull.Value;
            }
        }
    }
}
