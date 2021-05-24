using System;

namespace Kinetix.Monitoring
{
    public class Process
    {
        public Guid Id { get; } = Guid.NewGuid();

        public bool Error { get; internal set; }

        public bool Disabled { get; internal set; }

        public DateTime StartTime { get; } = DateTime.Now;

        public DateTime EndTime { get; internal set; }

        public int Duration => Convert.ToInt32((EndTime - StartTime).TotalMilliseconds);
    }
}
