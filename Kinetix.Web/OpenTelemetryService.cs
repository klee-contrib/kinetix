namespace Kinetix.Web;

/// <summary>
/// Description de service pour OpenTelemetry.
/// </summary>
public class OpenTelemetryService
{
    /// <summary>
    /// Nom du service ("Cloud Role Name" dans Application Insights).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Namespace du service.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Version du service.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Instance du service ("Cloud Role Instance" dans Application Insights).
    /// </summary>
    public string? InstanceId { get; set; }
}
