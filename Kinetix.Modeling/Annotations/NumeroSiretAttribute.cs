﻿using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Contrainte sur les numéros SIRET.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NumeroSiretAttribute : ValidationAttribute
{
    /// <summary>
    /// Longueur d'un numéro SIRET.
    /// </summary>
    public const int SiretLength = 14;

    /// <summary>
    /// Expression régulière permettant de valider le format SIRET.
    /// </summary>
    private static readonly string SiretRegex = string.Format(CultureInfo.InvariantCulture, "^[0-9]{{{0}}}$", SiretLength);

    /// <summary>
    /// Constructeur.
    /// </summary>
    public NumeroSiretAttribute()
    {
        ErrorMessageResourceType = typeof(SR);
        ErrorMessageResourceName = "SiretConstraintError";
    }

    /// <summary>
    /// Indique si le numéro SIRET est valide ou non.
    /// </summary>
    /// <param name="value">Numéro SIRET.</param>
    /// <returns><code>True</code> si le numéro SIRET est valide, <code>false</code> sinon.</returns>
    public override bool IsValid(object value)
    {
        var siret = value as string;

        if (string.IsNullOrEmpty(siret))
        {
            return false;
        }

        if (!Regex.IsMatch(siret, SiretRegex))
        {
            return false;
        }

        var sumOfDigits = 0;
        for (var i = 0; i < siret.Length; i++)
        {
            var tmp = (siret[i] - '0') * ((i + 1) % 2 + 1);
            sumOfDigits += tmp / 10 + tmp % 10;
        }

        return sumOfDigits % 10 == 0;
    }
}
