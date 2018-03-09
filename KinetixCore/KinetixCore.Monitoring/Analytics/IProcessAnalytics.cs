using System;


namespace KinetixCore.Monitoring
{
    public interface IProcessAnalytics
    {

        void Trace(string category, string name, Action<IProcessAnalyticsTracer> action, Action<AProcess> onCloseConsumer);

        O TraceWithReturn<O>(string category, string name, Func<IProcessAnalyticsTracer, O> action, Action<AProcess> onCloseConsumer) where O : class;

        ProcessAnalyticsTracer GetCurrentTracer();

        void BeginTrace(string category, string name, Action<IProcessAnalyticsTracer> action, Action<AProcess> onCloseConsumer);

        void EndTraceSuccess(Action<IProcessAnalyticsTracer> action, Action<AProcess> onCloseConsumer);

        void EndTraceFailure(Exception e, Action<IProcessAnalyticsTracer> action, Action<AProcess> onCloseConsumer);

    }
}

