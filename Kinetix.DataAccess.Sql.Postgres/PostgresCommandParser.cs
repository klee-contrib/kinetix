using Kinetix.DataAccess.Sql.Common;
using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Analyseur de requête SQL Dynamique.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="sqlManager">Composant injecté.</param>
internal class PostgresCommandParser(SqlManager sqlManager) : CommandParser(sqlManager)
{
    /// <inheritdoc />
    protected override bool IsNull(object parameter)
    {
        return DBNull.Value.Equals(((NpgsqlParameter)parameter).Value);
    }
}
