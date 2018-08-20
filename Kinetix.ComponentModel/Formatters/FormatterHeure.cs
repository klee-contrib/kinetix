﻿using System;
using System.Globalization;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Formatteur prenant en charge le rendu du type de données Heure.
    /// </summary>
    public class FormatterHeure : AbstractFormatter<TimeSpan?>
    {
        /// <summary>
        /// Conversion de la chaine de caractères vers le type TimeSpan.
        /// </summary>
        /// <param name="text">Texte à convertir.</param>
        /// <returns>TimeSpan.</returns>
        protected override TimeSpan? InternalConvertFromString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (TimeSpan.TryParseExact(text, FormatString, CultureInfo.CurrentCulture, TimeSpanStyles.None, out var result))
            {
                return result;
            }

            throw new FormatException(SR.ErrorFormatHeure);
        }

        /// <summary>
        /// Convertit le TimeSpan vers une chaine de caractères.
        /// </summary>
        /// <param name="value">TimeSpan.</param>
        /// <returns>Représentation textuelle.</returns>
        protected override string InternalConvertToString(TimeSpan? value)
        {
            return value.HasValue ? value.GetValueOrDefault().ToString(FormatString, DateTimeFormatInfo.CurrentInfo) : null;
        }
    }
}
