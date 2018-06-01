using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Kinetix.Monitoring.Abstractions;
using Microsoft.Extensions.Logging;

namespace KinetixCore.Monitoring
{
    public class ProcessAnalytics : IProcessAnalytics
    {
        private static readonly ThreadLocal<Stack<ProcessAnalyticsTracer>> threadLocalProcess = new ThreadLocal<Stack<ProcessAnalyticsTracer>>();

        private readonly ILoggerFactory _loggerFactory;

        public ProcessAnalytics(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Trace(string category, string name, Action<IProcessAnalyticsTracer> action, Action<IAProcess> onCloseConsumer)
        {
            using (ProcessAnalyticsTracer tracer = CreateTracer(category, name, onCloseConsumer))
            {
                try
                {
                    action.Invoke(tracer);
                    tracer.MarkAsSucceeded();
                }
                catch (Exception e)
                {
                    tracer.MarkAsFailed(e);
                    throw e;
                }
            }
        }

        public O TraceWithReturn<O>(string category, string name, Func<IProcessAnalyticsTracer, O> action, Action<IAProcess> onCloseConsumer)
        {
            using (ProcessAnalyticsTracer tracer = CreateTracer(category, name, onCloseConsumer))
            {
                try
                {
                    O result = action.Invoke(tracer);
                    tracer.MarkAsSucceeded();
                    return result;
                }
                catch (Exception e)
                {
                    tracer.MarkAsFailed(e);
                    throw e;
                }
            }
        }

        public IProcessAnalyticsTracer GetCurrentTracer()
        {
            return DoGetCurrentTracer();
        }

        private static ProcessAnalyticsTracer DoGetCurrentTracer()
        {
            if (threadLocalProcess.Value == null || threadLocalProcess.Value.Count == 0)
            {
                return null;
            }
            return threadLocalProcess.Value.Peek();
        }

        private static void Push(ProcessAnalyticsTracer analyticstracer)
        {
            Debug.Assert(analyticstracer != null);
            //---
            if (threadLocalProcess.Value == null)
            {
                threadLocalProcess.Value = new Stack<ProcessAnalyticsTracer>();
            }
            Debug.Assert(threadLocalProcess.Value.Count < 100, "More than 100 process deep. All processes must be closed.");
            threadLocalProcess.Value.Push(analyticstracer);
        }

        private ProcessAnalyticsTracer RemoveCurrentAndGetParentTracer()
        {
            threadLocalProcess.Value.Pop();
            ProcessAnalyticsTracer parentOpt = DoGetCurrentTracer();
            if (parentOpt == null)
            {
                threadLocalProcess.Value = null;
            }
            return parentOpt;
        }


        private ProcessAnalyticsTracer CreateTracer(string category, string name, Action<IAProcess> onCloseConsumer)
        {
            ProcessAnalyticsTracer analyticsTracer = new ProcessAnalyticsTracer(category, name, onCloseConsumer, () => RemoveCurrentAndGetParentTracer(), _loggerFactory);
            Push(analyticsTracer);
            return analyticsTracer;
        }

        public void BeginTrace(string category, string name, Action<IProcessAnalyticsTracer> action, Action<IAProcess> onCloseConsumer)
        {
            ProcessAnalyticsTracer analyticsTracer = CreateTracer(category, name, onCloseConsumer);
            action.Invoke(analyticsTracer);
        }

        public void EndTraceSuccess(Action<IProcessAnalyticsTracer> action, Action<IAProcess> onCloseConsumer)
        {
            using (ProcessAnalyticsTracer analyticsTracer = DoGetCurrentTracer())
            {
                action.Invoke(analyticsTracer);
                analyticsTracer.MarkAsSucceeded();
            }
        }

        public void EndTraceFailure(Exception e, Action<IProcessAnalyticsTracer> action, Action<IAProcess> onCloseConsumer)
        {
            using (ProcessAnalyticsTracer analyticsTracer = DoGetCurrentTracer())
            {
                action.Invoke(analyticsTracer);
                analyticsTracer.MarkAsFailed(e);
            }

        }

    }
}