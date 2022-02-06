using Npgsql;

namespace Kinetix.DataAccess.Sql.Postgres;

/// <summary>
/// Analyseur de requête SQL Dynamique.
/// </summary>
internal class PostgresCommandParser : CommandParser
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="sqlManager">Composant injecté.</param>
    public PostgresCommandParser(SqlManager sqlManager)
        : base(sqlManager)
    {
    }

    /// <inheritdoc />
    protected override bool IsNull(object parameter)
    {
        return DBNull.Value.Equals(((NpgsqlParameter)parameter).Value);
    }
}
