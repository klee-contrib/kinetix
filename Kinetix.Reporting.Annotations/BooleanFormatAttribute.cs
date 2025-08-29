namespace Kinetix.Reporting.Annotations;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class BooleanFormatAttribute(string @true, string @false) : Attribute
{
    public (string True, string False) Format { get; set; } = (@true, @false);
}
