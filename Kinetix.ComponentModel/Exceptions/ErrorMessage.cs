using System.Globalization;

namespace Kinetix.ComponentModel.Exceptions;

/// <summary>
/// Entrée d'une pile d'erreur.
/// </summary>
public sealed class ErrorMessage
{
    /// <summary>
    /// Crée une nouvelle entrée.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="code">Le code d'erreur.</param>
    public ErrorMessage(string message, string code = null)
    {
        Message = message;
        Code = code;
    }

    /// <summary>
    /// Crée une nouvelle entrée.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="code">Le code d'erreur.</param>
    public ErrorMessage(string message, string typeCible, int objetCibleId, string section = null, string code = null)
    {
        Message = message;
        Code = code;
        TypeCible = typeCible;
        ObjetCibleId = objetCibleId;
        SectionCode = section;
    }

    /// <summary>
    /// Crée une nouvelle entrée.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="code">Le code d'erreur.</param>
    internal ErrorMessage(string fieldName, string message, string code)
    {
        FieldName = fieldName;
        Message = message;
        Code = code;
    }

    /// <summary>
    /// Le code de l'erreur.
    /// </summary>
    public string Code
    {
        get;
        private set;
    }

    /// <summary>
    /// Le code de section.
    /// </summary>
    public string SectionCode
    {
        get;
        private set;
    }

    /// <summary>
    /// L'id cible
    /// </summary>
    public int? ObjetCibleId
    {
        get;
        private set;
    }

    /// <summary>
    /// Le type de cible
    /// </summary>
    public string TypeCible
    {
        get;
        private set;
    }

    /// <summary>
    /// Nom du champ en erreur.
    /// </summary>
    public string FieldName
    {
        get;
        private set;
    }

    /// <summary>
    /// Nom complet.
    /// </summary>
    public string FullFieldName => string.IsNullOrEmpty(ModelName)
        ? FieldName
        : string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ModelName, FieldName);

    /// <summary>
    /// Message d'erreur.
    /// </summary>
    public string Message
    {
        get;
        private set;
    }

    /// <summary>
    /// Nom du modèle concerné par l'erreur.
    /// </summary>
    public string ModelName
    {
        get;
        set;
    }
}
