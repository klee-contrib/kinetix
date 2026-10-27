namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class BooleanFormatAttribute(string @true, string @false) : Attribute
{
    public (string True, string False) Format { get; set; } = (@true, @false);
}
