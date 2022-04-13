namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Exception interceptée.
/// </summary>
public class InterceptedException : Exception
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="innerException">Exception interceptée.</param>
    public InterceptedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
