using KinetixCore.Monitoring.Analytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Services.Annotations
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceImplAttribute : AnalyticsAttribute
    {
        public ServiceImplAttribute(string category = "Service", string name = null) : base(category, name)
        {
        }
    }
}
