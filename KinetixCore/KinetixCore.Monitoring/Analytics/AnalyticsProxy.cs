using System;
using System.Reflection;
using Castle.DynamicProxy;
using Kinetix.Monitoring.Abstractions;

namespace KinetixCore.Monitoring.Analytics
{
    /// <summary>
    /// Proxy for Analytics Attributes. This proxy trace methods executions.
    /// </summary>
    public class AnalyticsProxy : IInterceptor
    {
        private IAnalyticsManager _analyticsManager;

        public AnalyticsProxy(IAnalyticsManager analyticsManager)
        {

            _analyticsManager = analyticsManager;
        }

        void IInterceptor.Intercept(IInvocation invocation)
        {
            try
            {
                MethodInfo targetMethod = invocation.GetConcreteMethodInvocationTarget();
                AnalyticsAttribute att = (AnalyticsAttribute)targetMethod.GetCustomAttribute(typeof(AnalyticsAttribute), false);

                if (att == null)
                {
                    att = (AnalyticsAttribute)targetMethod.DeclaringType.GetCustomAttribute(typeof(AnalyticsAttribute), false);
                }

                string category = att.Category;
                string name = att.Name ?? targetMethod.DeclaringType.FullName + "." + targetMethod.Name;
                _analyticsManager.Trace(att.Category, name, tracer => invocation.Proceed());

            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                throw ex.InnerException ?? ex;
            }
        }

    }
}
