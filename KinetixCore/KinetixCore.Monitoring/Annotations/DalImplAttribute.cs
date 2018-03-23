using KinetixCore.Monitoring.Analytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Services.Annotations
{
    /// <summary>
    /// 
    /// </summary>
    public class DalImplAttribute : AnalyticsAttribute
    {
        public DalImplAttribute(string category = "Dal", string name = null) : base(category, name)
        {
        }
    }
}
