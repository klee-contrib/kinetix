using System;
using System.Globalization;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Définition d'un formateur pour les décimaux avec gestion du séparateur par ',' ou '.'.
    /// </summary>
    public class FormatterDecimal : AbstractFormatter<decimal?>
    {
        /// <summary>
        /// Convertit une chaîne entrée d'un décimal en décimal.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override decimal? InternalConvertFromString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            text = text.Replace(".", ",");
            text = text.Replace(" ", string.Empty);
            if (decimal.TryParse(text.Trim(), NumberStyles.Number, NumberFormatInfo.CurrentInfo, out var result))
            {
                return result;
            }
            else
            {
                throw new FormatException(SR.ErrorFormatDecimal);
            }
        }

        /// <summary>
        /// Convertit un nombre entier en decimal pour affichage.
        /// </summary>
        /// <param name="value">Chaîne d'origine.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(decimal? value)
        {
            return value == null
                ? null
                : string.IsNullOrEmpty(FormatString)
                    ? value.Value.ToString(CultureInfo.CurrentCulture)
                    : value.Value.ToString(FormatString, CultureInfo.CurrentCulture);
        }
    }
}
