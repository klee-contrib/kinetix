using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Monitoring
{
    public interface IAnalyticsManager
    {
        void Trace(string category, string name, Action<IProcessAnalyticsTracer> action);

        O TraceWithReturn<O>(string category, string name, Func<IProcessAnalyticsTracer, O> action) where O : class;

        ProcessAnalyticsTracer GetCurrentTracer();

        void BeginTrace(string category, string name, Action<IProcessAnalyticsTracer> action);

        void EndTraceSuccess(Action<IProcessAnalyticsTracer> action);

        void EndTraceFailure(Exception e, Action<IProcessAnalyticsTracer> action);

    }
}
