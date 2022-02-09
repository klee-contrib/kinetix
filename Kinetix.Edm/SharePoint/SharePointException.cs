using System;
using System.Runtime.Serialization;

namespace Kinetix.Edm.SharePoint
{
    /// <summary>
    /// Exception générée par les appels à SharePoint.
    /// </summary>
    [Serializable]
    public class SharePointException : Exception
    {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public SharePointException()
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public SharePointException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public SharePointException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected SharePointException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
