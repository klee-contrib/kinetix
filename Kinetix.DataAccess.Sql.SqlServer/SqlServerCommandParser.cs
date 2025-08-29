using System.Data;
using Kinetix.DataAccess.Sql.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace Kinetix.DataAccess.Sql.SqlServer;

/// <summary>
/// Analyseur de requête SQL Dynamique.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="sqlManager">Composant injecté.</param>
internal class SqlServerCommandParser(SqlManager sqlManager) : CommandParser(sqlManager)
{
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
