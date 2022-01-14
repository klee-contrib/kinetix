﻿using System;
using System.Runtime.Serialization;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Exception générée par un timeout de la base de données.
    /// </summary>
    [Serializable]
    public class SqlTimeoutException : Exception
    {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public SqlTimeoutException()
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public SqlTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public SqlTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected SqlTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
