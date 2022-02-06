using System.Data;
using System.Text;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Factory d'adapter.
/// </summary>
/// <typeparam name="T">Type de données.</typeparam>
internal static class DataRecordAdapterManager<T>
    where T : new()
{
    private static readonly Dictionary<string, IDataRecordAdapter<T>> _adaptorMap = new();

    /// <summary>
    /// Crée un adapteur.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <returns>Adapter.</returns>
    internal static IDataRecordAdapter<T> CreateAdapter(IDataRecord record)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < record.FieldCount; ++i)
        {
            sb.Append(record.GetName(i)).Append(',');
        }

        IDataRecordAdapter<T> adapter;
        lock (_adaptorMap)
        {
            if (!_adaptorMap.TryGetValue(sb.ToString(), out adapter))
            {
                adapter = InternalCreateAdapter(record);
                _adaptorMap[sb.ToString()] = adapter;
            }
        }

        return adapter;
    }

    /// <summary>
    /// Crée un adapteur.
    /// </summary>
    /// <param name="record">Record.</param>
    /// <returns>Adapter.</returns>
    private static IDataRecordAdapter<T> InternalCreateAdapter(IDataRecord record)
    {
        return (IDataRecordAdapter<T>)DataRecordAdapterFactory.Instance.CreateAdapter(record, typeof(T));
    }
}
