using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Monitoring
{
    public interface IAnalyticsConnectorPlugin
    {

        /// <summary>
        /// Method to add a monitoring process.
        /// </summary>
        /// <param name="process"></param>
        void Add(AProcess process);

    }
}
