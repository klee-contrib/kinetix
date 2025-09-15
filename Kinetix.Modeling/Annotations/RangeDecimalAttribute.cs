using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Class attribute for range of decimal objects.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="minimum">Minium value.</param>
/// <param name="maximum">Maximum value.</param>
/// <param name="isMinimumIncluded">Defines whether minimum is included.</param>
/// <param name="isMaximumIncluded">Defines whether maximum is included.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class RangeDecimalAttribute(
    double minimum,
    double maximum,
    bool isMinimumIncluded,
    bool isMaximumIncluded
) : RangeAttribute(minimum, maximum)
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="minimum">Minium value.</param>
    /// <param name="maximum">Maximum value.</param>
    public RangeDecimalAttribute(double minimum, double maximum)
        : this(minimum, maximum, isMinimumIncluded: true, isMaximumIncluded: true) { }

    /// <summary>
    /// Defines whether is the minimum is included in the range.
    /// </summary>
    public bool IsMinimumIncluded { get; private set; } = isMinimumIncluded;

    /// <summary>
    /// Defines whether is the maximum is included in the range.
    /// </summary>
    public bool IsMaximumIncluded { get; private set; } = isMaximumIncluded;

    /// <summary>
    /// Format error message.
    /// </summary>
    /// <param name="name">Error value.</param>
    /// <returns>Formated error message.</returns>
    public override string FormatErrorMessage(string name)
    {
        var s = string.Empty;
        var i = (IsMaximumIncluded ? 10 : 0) + (IsMinimumIncluded ? 1 : 0);
        switch (i)
        {
            case 0:
                s = SR.ErrorRangeDecimalOpen;
                break;
            case 1:
                s = SR.ErrorRangeDecimalRightOpen;
                break;
            case 10:
                s = SR.ErrorRangeDecimalLeftOpen;
                break;
            case 11:
                return base.FormatErrorMessage(name);
        }

        return string.Format(CultureInfo.CurrentCulture, s, Minimum, Maximum);
    }

    /// <summary>
    /// True if object is valid.
    /// </summary>
    /// <param name="value">Obect to test.</param>
    /// <returns>Boolean.</returns>
    public override bool IsValid(object value)
    {
        var s = value as string;
        return string.IsNullOrEmpty(s)
            ? base.IsValid(value)
            : decimal.TryParse(s, out var d)
                && base.IsValid(s.Replace(',', '.'))
                && (IsMinimumIncluded || (double)d > (double)Minimum)
                && (IsMaximumIncluded || (double)d < (double)Maximum);
    }
}
