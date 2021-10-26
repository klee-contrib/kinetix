using System;

namespace Kinetix.Reporting.Annotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BooleanFormatAttribute : Attribute
    {
        public BooleanFormatAttribute(string @true, string @false)
        {
            Format = (@true, @false);
        }

        public (string True, string False) Format { get; set; }
    }
}
