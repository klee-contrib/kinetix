using System;
using System.Runtime.Serialization;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Exception générée par les appels base de données.
    /// </summary>
    [Serializable]
    public class SqlDataException : Exception
    {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public SqlDataException()
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public SqlDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public SqlDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected SqlDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
