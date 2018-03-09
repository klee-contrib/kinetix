using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Monitoring
{
    public interface IAnalyticsConnectorPlugin
    {
        
        void Add(AProcess process);


        /*void Add(Metric metric);

        void Add(HealthCheck healthCheck);*/
    }
}
