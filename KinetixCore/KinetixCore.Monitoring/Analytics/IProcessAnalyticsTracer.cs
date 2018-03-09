namespace KinetixCore.Monitoring
{
    public interface IProcessAnalyticsTracer
    {
        
        IProcessAnalyticsTracer IncMeasure(string name, double value);

        IProcessAnalyticsTracer SetMeasure(string name, double value);

        IProcessAnalyticsTracer AddTag(string name, string value);

    }
}