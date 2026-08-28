using System.Globalization;

namespace Kinetix.Modeling.Exceptions;

/// <summary>
/// Pile d'erreur.
/// </summary>
public sealed class ErrorMessageCollection : List<ErrorMessage>
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    public ErrorMessageCollection()
        : base() { }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="erreurs">Liste d'erreurs.</param>
    public ErrorMessageCollection(IEnumerable<ErrorMessage> erreurs)
        : base(erreurs) { }

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="erreurs">Liste d'erreurs.</param>
    public ErrorMessageCollection(IEnumerable<string> erreurs)
        : base()
    {
        foreach (var err in erreurs)
        {
            AddBusinessException(err);
        }
    }

    /// <summary>
    /// Indique si la pile contient des erreurs.
    /// </summary>
    public bool HasError => Count > 0;

    /// <summary>
    /// Ajoute une entrée à la pile d'erreur.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="errorMessage">Message d'erreur.</param>
    public void Add(string fieldName, string? errorMessage)
    {
        Add(new(fieldName, errorMessage, code: null));
    }

    /// <summary>
    /// Ajoute une entrée à la pile d'erreur.
    /// </summary>
    /// <param name="rownum">Numéro de la ligne en erreur.</param>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="errorMessage">Message d'erreur.</param>
    public void Add(int rownum, string fieldName, string errorMessage)
    {
        Add(new("[" + rownum.ToString(CultureInfo.InvariantCulture) + "]." + fieldName, errorMessage, code: null));
    }

    /// <summary>
    /// Ajoute une exception à la liste des erreurs.
    /// </summary>
    /// <param name="ce">Exception.</param>
    public void AddBusinessException(BusinessException ce)
    {
        ArgumentNullException.ThrowIfNull(ce);

        if (ce.Errors.Any())
        {
            AddErrorStack(string.Empty, ce.Errors);
        }
        else if (ce.Property != null)
        {
            Add(ce.Property.PropertyName, ce.Message);
        }
        else
        {
            Add(string.Empty, ce.Message);
        }
    }

    /// <summary>
    /// Ajoute une exception à la liste des erreurs.
    /// </summary>
    /// <param name="message">Le message de l'exception.</param>
    public void AddBusinessException(string message)
    {
        AddBusinessException(new BusinessException(message));
    }

    /// <summary>
    /// Ajoute une pile d'erreur à la pile courante.
    /// </summary>
    /// <param name="fieldPrefix">Préfixe à utiliser.</param>
    /// <param name="errorCollection">Liste des erreurs.</param>
    public void AddErrorStack(string fieldPrefix, ErrorMessageCollection errorCollection)
    {
        ArgumentNullException.ThrowIfNull(errorCollection);

        foreach (var entry in errorCollection)
        {
            Add(fieldPrefix + entry.FieldName, entry.Message);
        }
    }

    /// <summary>
    /// Lève une erreur si des erreurs ont été détectées.
    /// </summary>
    public void Throw()
    {
        if (HasError)
        {
            throw new BusinessException(this);
        }
    }
}
