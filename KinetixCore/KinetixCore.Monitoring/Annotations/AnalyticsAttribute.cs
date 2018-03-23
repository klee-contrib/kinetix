using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Monitoring.Analytics
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false)]
    public class AnalyticsAttribute : Attribute
    {

        public string Category { get; }
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        public AnalyticsAttribute(string category, string name = null)
        {
            Category = category;
            Name = name;
        }
    }
}

