using System;
using System.Globalization;

namespace Kinetix.ComponentModel.Formatters
{
    /// <summary>
    /// Définition d'un formateur de date affichant uniquement le mois et l'année.
    /// </summary>
    public class FormatterMoisAnnee : FormatterDate
    {
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

            return Convert.ToDateTime(text, CultureInfo.CurrentCulture);
        }
    }
}
