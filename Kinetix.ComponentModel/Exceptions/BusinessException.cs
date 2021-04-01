﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Exceptions
{
    /// <summary>
    /// Erreur métier.
    /// </summary>
    [Serializable]
    public class BusinessException : Exception
    {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public BusinessException()
        {
        }

        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        /// <param name="errorCollection">Pile d'erreur.</param>
        public BusinessException(ErrorMessageCollection errorCollection)
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
        public BusinessException(IEnumerable<string> messageList)
        {
            var erreurs = new ErrorMessageCollection();
            foreach (var message in messageList)
            {
                erreurs.AddConstraintException(message);
            }

            Errors = erreurs;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="messageList">Liste de messages d'erreurs.</param>
        /// <param name="code">Le code de l'erreur.</param>
        public BusinessException(IEnumerable<ErrorMessage> messageList, string code = null)
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
        {
            Errors = new ErrorMessageCollection();
            Errors.AddEntry(fieldName, message);
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="messageParameters">Message parameters.</param>
        public BusinessException(string message, Dictionary<string, ErrorMessageParameter> messageParameters)
            : base(message)
        {
            MessageParameters = messageParameters;
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
        /// <param name="property">Propriété associée à la violation de contrainte.</param>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public BusinessException(BeanPropertyDescriptor property, string message, Exception innerException)
            : base(message, innerException)
        {
            Property = property;
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
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                Property = (BeanPropertyDescriptor)info.GetValue("Property", typeof(BeanPropertyDescriptor));
            }
        }

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

        /// <summary>
        /// Sérialise l'exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                base.GetObjectData(info, context);
                info.AddValue("Property", Property);
            }
        }
    }
}
