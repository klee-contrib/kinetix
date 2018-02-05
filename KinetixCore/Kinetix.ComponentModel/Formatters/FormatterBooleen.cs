using System;
using System.Text.RegularExpressions;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Définition d'un formateur de booléen.
    /// </summary>
    public class FormatterBooleen : AbstractFormatter<bool?>
    {
        /// <summary>
        /// Valeur True.
        /// </summary>
        public static string True => SR.TextBooleanYes;

        /// <summary>
        /// Valeur False.
        /// </summary>
        public static string False => SR.TextBooleanNo;

        /// <summary>
        /// Valeur non définie.
        /// </summary>
        public static string Undefined => SR.TextBooleanUndefined;

        /// <summary>
        /// Convertit une chaine de caractère Oui ou Non en booléen.
        /// </summary>
        /// <param name="text">Texte du booléen.</param>
        /// <returns>La valeur booléenne correspondant au texte.</returns>
        /// <todo type="IGNORE" who="SEY">Internationalisation.</todo>
        protected override bool? InternalConvertFromString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            // conversion en booléen
            if (!bool.TryParse(text, out bool value))
            {

                // vérification du format de saisie
                string numeroFormat = @"^(true|True|oui|Oui|1)$";
                if (Regex.IsMatch(text, numeroFormat))
                {
                    return true;
                }

                numeroFormat = @"^(false|False|non|Non|0)$";
                if (Regex.IsMatch(text, numeroFormat))
                {
                    return false;
                }

                throw new FormatException(SR.ErrorFormatBooleen);
            }

            return value;
        }

        /// <summary>
        /// Convertit un booléen en chaîne de caractères.
        /// </summary>
        /// <param name="value">Représentation de la valeur booléenne.</param>
        /// <returns>La valeur du booléen en string.</returns>
        protected override string InternalConvertToString(bool? value)
        {
            if (!value.HasValue)
            {
                return Undefined;
            }

            return value.Value ? True : False;
        }
    }
}
