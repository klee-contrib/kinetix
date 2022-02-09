using System;

namespace Kinetix.Reporting.Annotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DateFormatAttribute : Attribute
    {
        public DateFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}
