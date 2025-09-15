namespace Kinetix.Search.Models;

/// <summary>
/// Définition de tri.
/// </summary>
public class SortInput
{
    public required string FieldName { get; set; }

    public bool SortDesc { get; set; }
}
