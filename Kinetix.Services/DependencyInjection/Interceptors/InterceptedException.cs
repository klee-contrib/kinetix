namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Exception interceptée.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="message">Message.</param>
/// <param name="innerException">Exception interceptée.</param>
public class InterceptedException(string message, Exception innerException) : Exception(message, innerException)
{
}
