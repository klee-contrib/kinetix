namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DateFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}
