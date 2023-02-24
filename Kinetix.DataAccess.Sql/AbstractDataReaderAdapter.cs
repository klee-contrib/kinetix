using System.Data;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Base des adapteurs.
/// </summary>
public abstract class AbstractDataReaderAdapter
{
    /// <summary>
    /// Retourne un Boolean.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Boolean.</returns>
    public static bool? ReadBoolean(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetBoolean(idx);
    }

    /// <summary>
    /// Retourne un Byte.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Byte.</returns>
    public static byte? ReadByte(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetByte(idx);
    }

    /// <summary>
    /// Retourne un char.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Char.</returns>
    public static char? ReadChar(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetChar(idx);
    }

    /// <summary>
    /// Retourne un DateTime.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>DateTime.</returns>
    public static DateTime? ReadDateTime(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetDateTime(idx);
    }

    /// <summary>
    /// Retourne un decimal.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Decimal.</returns>
    public static decimal? ReadDecimal(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetDecimal(idx);
    }

    /// <summary>
    /// Retourne un double.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Double.</returns>
    public static double? ReadDouble(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetDouble(idx);
    }

    /// <summary>
    /// Retourne un float.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Float.</returns>
    public static float? ReadFloat(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetFloat(idx);
    }

    /// <summary>
    /// Retourne un guid.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Guid.</returns>
    public static Guid? ReadGuid(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetGuid(idx);
    }

    /// <summary>
    /// Retourne un int32.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Entier.</returns>
    public static int? ReadInt(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetInt32(idx);
    }

    /// <summary>
    /// Retourne un long.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Long.</returns>
    public static long? ReadLong(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetInt64(idx);
    }

    /// <summary>
    /// Retourne un Boolean.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Boolean.</returns>
    public static bool ReadNonNullableBoolean(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            throw new ArgumentNullException(nameof(record));
        }

        return record.GetBoolean(idx);
    }

    /// <summary>
    /// Retourne un objet.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Tableau de char..</returns>
    public static T ReadObject<T>(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return default;
        }

        return (T)record.GetValue(idx);
    }

    /// <summary>
    /// Retourne un short.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>Short.</returns>
    public static short? ReadShort(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetInt16(idx);
    }

    /// <summary>
    /// Retourne un string.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <param name="idx">Index.</param>
    /// <returns>String.</returns>
    public static string ReadString(IDataRecord record, int idx)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (record.IsDBNull(idx))
        {
            return null;
        }

        return record.GetString(idx);
    }
}
