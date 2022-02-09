namespace Kinetix.Monitoring
{
    public class Counter
    {
        public string Code { get; set; }
        public string Label { get; set; }
        public int WarningThreshold { get; set; }
        public int CriticalThreshold { get; set; }
    }
}
