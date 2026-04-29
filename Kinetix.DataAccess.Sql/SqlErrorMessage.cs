namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Classe formalisant la remontée d'une erreur SQL une fois parsée.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="message">Message d'erreur.</param>
/// <param name="code">Code de l'erreur.</param>
public sealed class SqlErrorMessage(string? message, string code)
{
    /// <summary>
    /// Obtient le message d'erreur.
    /// </summary>
    public string? Message { get; private set; } = message;

    /// <summary>
    /// Obtient le code d'erreur.
    /// </summary>
    public string Code { get; private set; } = code;
}
