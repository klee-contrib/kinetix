using System;
using System.Collections.Generic;

namespace Kinetix.ComponentModel.Exceptions
{
    /// <summary>
    /// Exception liée à une entité.
    /// </summary>
    public class EntityException : Exception
    {
        /// <summary>
        /// Clé de l'objet de retour JSON contenant le code de l'erreur.
        /// </summary>
        public const string CodeKey = "code";

        /// <summary>
        /// Clé de l'objet de retour JSON contenant les erreurs globales.
        /// </summary>
        public const string GlobalErrorKey = "globalErrors";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="error">Error message.</param>
        public EntityException(string error)
        {
            AddError(error);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldPath">Name of the field (referentiel.user.nom).</param>
        /// <param name="error">Error message.</param>
        public EntityException(string fieldPath, string error)
        {
            AddError(fieldPath, error);
        }

        /// <summary>
        /// Return the list of errors.
        /// </summary>
        public IDictionary<string, object> ErrorList { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Add an error for a field in a row in a list.
        /// </summary>
        /// <param name="fieldPath">List field path.</param>
        /// <param name="rowId">Id of the row (given by the key of the ColletionChanges object).</param>
        /// <param name="rowFieldPath">Row field path.</param>
        /// <param name="error">Error message.</param>
        public void AddCollectionError(string fieldPath, string rowId, string rowFieldPath, string error)
        {
            if (!ErrorList.ContainsKey(fieldPath))
            {
                ErrorList.Add(fieldPath, new Dictionary<string, IDictionary<string, string>>());
            }

            var errorDetail = (IDictionary<string, IDictionary<string, string>>)ErrorList[fieldPath];
            if (!errorDetail.ContainsKey(rowId))
            {
                errorDetail.Add(rowId, new Dictionary<string, string>());
            }

            errorDetail[rowId].Add(rowFieldPath, error);
        }

        /// <summary>
        /// Add a general error.
        /// </summary>
        /// <param name="message">Error message.</param>
        public void AddError(string message)
        {
            if (!ErrorList.ContainsKey(GlobalErrorKey))
            {
                ErrorList.Add(GlobalErrorKey, new List<string>());
            }

            ((ICollection<string>)ErrorList[GlobalErrorKey]).Add(message);
        }

        /// <summary>
        /// Add a field error.
        /// </summary>
        /// <param name="fieldPath">Name of the field (referentiel.user.nom).</param>
        /// <param name="error">Error message.</param>
        public void AddError(string fieldPath, string error)
        {
            ErrorList.Add(fieldPath, error);
        }

        /// <summary>
        /// Throw the exception if there is at least one error.
        /// </summary>
        public void ThrowIfError()
        {
            if (ErrorList.Count > 0)
            {
                throw this;
            }
        }
    }
}