using System.Data.Common;
using System.Globalization;

namespace Kinetix.DataAccess.Sql.Common;

public abstract partial class BaseSqlCommand
{
    /// <summary>
    /// Exécute la commande de mise à jour de données.
    /// </summary>
    /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
    /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
    /// <returns>Nombre de ligne impactées.</returns>
    public int ExecuteNonQuery(int minRowsAffected, int maxRowsAffected)
    {
        var rowsAffected = ExecuteNonQuery();
        if (rowsAffected < minRowsAffected)
        {
            throw rowsAffected == 0
                ? new SqlDataException(SR.ExceptionZeroRowAffected)
                : new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooFewRowsAffected, rowsAffected)
                );
        }

        if (rowsAffected > maxRowsAffected)
        {
            throw new SqlDataException(
                string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooManyRowsAffected, rowsAffected)
            );
        }

        return rowsAffected;
    }

    /// <summary>
    /// Exécute la commande de mise à jour de données.
    /// </summary>
    /// <returns>Nombre de ligne impactées.</returns>
    public int ExecuteNonQuery()
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, queryParameter: null);
            return InnerCommand.ExecuteNonQuery();
        }
        catch (DbException sqle)
        {
            throw listener.HandleException(sqle);
        }
        finally
        {
            listener.Dispose();
        }
    }

    /// <summary>
    /// Exécute une commande de selection et retourne un dataReader.
    /// </summary>
    /// <returns>DataReader.</returns>
    public SqlDataReader ExecuteReader()
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, QueryParameters);
            return new SqlDataReader(InnerCommand.ExecuteReader(), QueryParameters);
        }
        catch (DbException sqle)
        {
            throw listener.HandleException(sqle);
        }
        finally
        {
            listener.Dispose();
        }
    }

    /// <summary>
    /// Exécute une requête de select et retourne la première valeur
    /// de la première ligne.
    /// </summary>
    /// <returns>Retourne la valeur ou null.</returns>
    public object? ExecuteScalar()
    {
        var listener = GetSqlCommandListener();
        try
        {
            CommandParser.ParseCommand(InnerCommand, _parserKey, queryParameter: null);
            var value = InnerCommand.ExecuteScalar();
            return (value == DBNull.Value) ? null : value;
        }
        catch (DbException sqle)
        {
            throw listener.HandleException(sqle);
        }
        finally
        {
            listener.Dispose();
        }
    }

    /// <summary>
    /// Exécute une requête de select et retour la première valeur
    /// de la première ligne.
    /// </summary>
    /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
    /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
    /// <returns>Retourne la valeur ou null.</returns>
    public object? ExecuteScalar(int minRowsAffected, int maxRowsAffected)
    {
        using var reader = ExecuteReader();
        if (reader.Read())
        {
            var rowsAffected = reader.RecordsAffected;
            if (rowsAffected > maxRowsAffected)
            {
                throw new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooManyRowsAffected, rowsAffected)
                );
            }

            if (rowsAffected < minRowsAffected)
            {
                throw new SqlDataException(
                    string.Format(CultureInfo.CurrentCulture, SR.ExceptionTooFewRowsAffected, rowsAffected)
                );
            }

            return reader.GetValue(0);
        }

        throw new SqlDataException(SR.ExceptionZeroRowAffected);
    }
}
