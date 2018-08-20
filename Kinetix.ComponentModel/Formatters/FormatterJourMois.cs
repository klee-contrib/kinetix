using System;
using System.Globalization;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Définition d'un formateur de date affichant uniquement le jour et le mois.
    /// </summary>
    public class FormatterJourMois : FormatterDate
    {
        /// <summary>
        /// Convertit un string en date.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Date.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        protected override DateTime? InternalConvertFromString(string text)
        {
            return string.IsNullOrEmpty(text)
                ? null
                : (DateTime?)Convert.ToDateTime(text, CultureInfo.CurrentCulture);
        }
    }
}
