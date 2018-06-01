using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Monitoring.Abstractions;

namespace KinetixCore.Monitoring
{
    public class AnalyticsManager : IAnalyticsManager
    {
        private IProcessAnalytics _processAnalytics;
        private readonly IEnumerable<IAnalyticsConnectorPlugin> _processConnectorPlugins;

        private bool _enabled;

        public AnalyticsManager(IProcessAnalytics processAnalytics, IEnumerable<IAnalyticsConnectorPlugin> plugins)
        {
            _processAnalytics = processAnalytics;
            _processConnectorPlugins = plugins;
            _enabled = true;
        }

        public void Trace(string category, string name, Action<IProcessAnalyticsTracer> action)
        {
            _processAnalytics.Trace(category, name, action, OnClose);
        }

        public O TraceWithReturn<O>(string category, string name, Func<IProcessAnalyticsTracer, O> action)
        {
            return _processAnalytics.TraceWithReturn(category, name, action, OnClose);
        }

        public void BeginTrace(string category, string name, Action<IProcessAnalyticsTracer> action)
        {
            _processAnalytics.BeginTrace(category, name, action, OnClose);
        }

        public void EndTraceSuccess(Action<IProcessAnalyticsTracer> action)
        {
            _processAnalytics.EndTraceSuccess(action, OnClose);
        }

        public void EndTraceFailure(Exception e, Action<IProcessAnalyticsTracer> action)
        {
            _processAnalytics.EndTraceFailure(e, action, OnClose);
        }

        public IProcessAnalyticsTracer GetCurrentTracer()
        {
            if (!_enabled)
            {
                return null;
            }
            return _processAnalytics.GetCurrentTracer();
        }

        private void OnClose(IAProcess process)
        {
            Debug.Assert(process != null);
            //---
            foreach (var plugin in _processConnectorPlugins)
            {
                plugin.Add(process);
            }
        }
    }
}
