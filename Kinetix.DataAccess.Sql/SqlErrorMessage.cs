namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Classe formalisant la remontée d'une erreur SQL une fois parsée.
/// </summary>
public sealed class SqlErrorMessage
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="code">Code de l'erreur.</param>
    public SqlErrorMessage(string message, string code)
    {
        Message = message;
        Code = code;
    }

    /// <summary>
    /// Obtient le message d'erreur.
    /// </summary>
    public string Message
    {
        get;
        private set;
    }

    /// <summary>
    /// Obtient le code d'erreur.
    /// </summary>
    public string Code
    {
        get;
        private set;
    }
}
