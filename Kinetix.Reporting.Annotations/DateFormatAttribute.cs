namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DateFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}
