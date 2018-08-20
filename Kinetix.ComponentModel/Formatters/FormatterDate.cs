using System;
using System.Globalization;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Définition d'un formateur de date.
    /// </summary>
    public class FormatterDate : AbstractFormatter<DateTime?>
    {
        /// <summary>
        /// Tableau des formats de String acceptés.
        /// </summary>
        private static readonly string[] _stringFormats = { "dd/MM/yyyy", "ddMMyyyy", "dd/MM/yy", "ddMMyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yy", "dd/M/yy", "d/M/yy" };

        /// <summary>
        /// Convertit un string en date.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Date.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        protected override DateTime? InternalConvertFromString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(text, _stringFormats, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);
            }
            catch (FormatException)
            {
                var array = text.ToCharArray();
                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i] >= '2' && array[i] <= '9')
                    {
                        array[i] = '1';
                    }
                }

                if (DateTime.TryParseExact(new string(array), _stringFormats, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var date))
                {
                    throw new FormatException(SR.ErrorFormatDateValue);
                }

                throw new FormatException(SR.ErrorFormatDate);
            }
        }

        /// <summary>
        /// Convertit une date en string.
        /// </summary>
        /// <param name="value">Date.</param>
        /// <returns>Représentation textuelle de la date.</returns>
        protected override string InternalConvertToString(DateTime? value)
        {
            return value.HasValue ? value.GetValueOrDefault().ToString(FormatString, DateTimeFormatInfo.CurrentInfo) : null;
        }
    }
}
