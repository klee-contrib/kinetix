using System;

namespace Kinetix.Reporting.Annotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NumberFormatAttribute : Attribute
    {
        public NumberFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}
