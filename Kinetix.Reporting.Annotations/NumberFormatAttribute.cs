namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NumberFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}
