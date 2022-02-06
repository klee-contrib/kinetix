namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Seletion des columns à sauvegarder ou à ignorer pour la sauvegarder.
/// </summary>
public class ColumnSelector
{
    /// <summary>
    /// Default constructeur.
    /// </summary>
    public ColumnSelector()
    {
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="columnList">List of selected columns.</param>
    public ColumnSelector(params Enum[] columnList)
    {
        Add(columnList);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="columnList">List of selected columns.</param>
    public ColumnSelector(params string[] columnList)
    {
        Add(columnList);
    }

    /// <summary>
    /// Get the selected columns.
    /// </summary>
    public ICollection<string> ColumnList { get; } = new List<string>();

    /// <summary>
    /// Add columns.
    /// </summary>
    /// <param name="columnList">List of selected columns.</param>
    public void Add(params Enum[] columnList)
    {
        foreach (var col in columnList)
        {
            ColumnList.Add(col.ToString());
        }
    }

    /// <summary>
    /// Add columns.
    /// </summary>
    /// <param name="columnList">List of selected columns.</param>
    public void Add(params string[] columnList)
    {
        foreach (var col in columnList)
        {
            ColumnList.Add(col);
        }
    }
}
