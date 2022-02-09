using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Memory
{
    public class ProcessSummary
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int TotalDuration { get; set; }
        public int MeanDuration { get; set; }
        public int MaxDuration { get; set; }
        public IEnumerable<Guid> Processes { get; set; }
    }
}
