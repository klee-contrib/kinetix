using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.AnalyticsServer
{
    public class LogMessage
    {
        
        public string AppName { get; set; }

        public string Host { get; set; }

        [JsonProperty("event")]
        public AProcess LogEvent { get; set; }

    }
}
