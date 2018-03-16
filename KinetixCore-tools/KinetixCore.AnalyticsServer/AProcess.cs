using System;
using System.Collections.Generic;

namespace KinetixCore.AnalyticsServer
{
    public class AProcess
    {

        public string Category { get; }
        public string Name { get; }
        public long Start { get; }
        public long End { get; }
        public IDictionary<string, double> Measures { get; } = new Dictionary<string, double>();
        public IDictionary<string, string> Tags { get; } = new Dictionary<string, string>();
        public IList<AProcess> SubProcesses { get; } = new List<AProcess>();

        public AProcess(string category, string name, long start, long end, IDictionary<string, double> measures, IDictionary<string, string> tags, IList<AProcess> subProcesses)
        {
            this.Category = category;
            this.Name = name;
            this.Start = start;
            this.End = end;
            this.Measures = measures;
            this.Tags = tags;
            this.SubProcesses = subProcesses;
        }

        public long DurationMillis() => End - Start;

    }
}

