namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class NumberFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}
