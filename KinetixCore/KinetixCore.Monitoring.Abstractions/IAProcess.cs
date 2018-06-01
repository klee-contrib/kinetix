using System.Collections.Generic;

namespace Kinetix.Monitoring.Abstractions
{
    public interface IAProcess
    {
        string Category { get; }
        long End { get; }
        IDictionary<string, double> Measures { get; }
        string Name { get; }
        long Start { get; }
        IList<IAProcess> SubProcesses { get; }
        IDictionary<string, string> Tags { get; }

        long DurationMillis();
    }
}