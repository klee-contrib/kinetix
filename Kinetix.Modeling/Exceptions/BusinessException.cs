namespace Kinetix.Modeling.Exceptions;

/// <summary>
/// Erreur métier.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Crée un nouvelle exception.
    /// </summary>
    /// <param name="errorCollection">Pile d'erreur.</param>
    public BusinessException(ErrorMessageCollection errorCollection)
        : base(string.Empty)
    {
        Errors = errorCollection;
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    public BusinessException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="messageList">Liste de messages d'erreurs.</param>
    /// <param name="code">Le code de l'erreur.</param>
    public BusinessException(IEnumerable<ErrorMessage> messageList, string code = null)
        : base(string.Empty)
    {
        Errors = new ErrorMessageCollection(messageList);
        Code = code;
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="property">Propriété associée à la violation de contrainte.</param>
    /// <param name="message">Description de l'exception.</param>
    public BusinessException(BeanPropertyDescriptor property, string message)
        : base(message)
    {
        Property = property;
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="fieldName">Nom du champ en erreur.</param>
    /// <param name="message">Message d'erreur.</param>
    public BusinessException(string fieldName, string message)
        : base(string.Empty)
    {
        Errors = new ErrorMessageCollection();
        Errors.AddEntry(fieldName, message);
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    /// <param name="innerException">Exception source.</param>
    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="fieldName">Nom du champ en erreur.</param>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="code">Code d'erreur.</param>
    public BusinessException(string fieldName, string message, string code)
        : this(fieldName, message)
    {
        Code = code;
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    /// <param name="code">Code d'erreur.</param>
    /// <param name="innerException">Exception source.</param>
    public BusinessException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="errors">Liste d'erreurs.</param>
    /// <param name="innerException">Exception source.</param>
    public BusinessException(IEnumerable<ErrorMessage> errors, Exception innerException)
        : base(string.Empty, innerException)
    {
        Errors = new ErrorMessageCollection(errors);
    }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public override string Message
    {
        get => string.IsNullOrEmpty(base.Message) && Errors.HasError ? string.Join(", ", Errors.Select(s => s.Message)) : base.Message;
    }


    /// <summary>
    /// Message d'erreur d'origine
    /// </summary>
    public string BaseMessage => base.Message;

    /// <summary>
    /// Code d'erreur.
    /// </summary>
    public string Code
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la pile des erreurs.
    /// </summary>
    public ErrorMessageCollection Errors
    {
        get;
        private set;
    }

    /// <summary>
    /// List of parameters to inject in the message describing the exception.
    /// </summary>
    public Dictionary<string, ErrorMessageParameter> MessageParameters { get; } = new Dictionary<string, ErrorMessageParameter>();

    /// <summary>
    /// Retourne la propriété associée à la violation de contrainte.
    /// </summary>
    public BeanPropertyDescriptor Property
    {
        get;
        private set;
    }
}
