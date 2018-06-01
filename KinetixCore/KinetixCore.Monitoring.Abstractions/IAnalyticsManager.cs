using System;

namespace Kinetix.Monitoring.Abstractions
{
    public interface IAnalyticsManager
    {
        /// <summary>
        /// Traces a process and collects metrics during its execution.
        /// A traced process is stored by categories.
        /// </summary>
        /// <param name="category">the category of the process</param>
        /// <param name="name">the name of the process</param>
        /// <param name="action">the function to execute within the tracer</param>
        void Trace(string category, string name, Action<IProcessAnalyticsTracer> action);

        /// <summary>
        /// Traces a process that has a return value(and collects metrics during its execution).
        /// A traced process is stored by categories.
        /// </summary>
        /// <typeparam name="O"></typeparam>
        /// <param name="category">the category of the process</param>
        /// <param name="name">the name of the process</param>
        /// <param name="action">the action to execute within the tracer</param>
        /// <returns>the result of the traced function</returns>
        O TraceWithReturn<O>(string category, string name, Func<IProcessAnalyticsTracer, O> action);

        /// <summary>
        /// Return the current tracer if it has been created before
        /// </summary>
        /// <returns>the current tracer if it has been created before</returns>
        IProcessAnalyticsTracer GetCurrentTracer();

        /// <summary>
        /// Begin to trace a process (and collects metrics during its execution).
        /// A traced process is stored by categories.
        /// This method is used for tracing but with the asynchronous style pattern (before/after).
        /// </summary>
        /// <param name="category">the category of the process</param>
        /// <param name="name">the name of the process</param>
        /// <param name="action">the action to execute within the tracer</param>
        void BeginTrace(string category, string name, Action<IProcessAnalyticsTracer> action);

        /// <summary>
        /// Finish to trace a process and mark the execution as successful (and collects metrics during its execution).
        /// A traced process is stored by categories.
        /// This method is used for tracing but with the asynchronous style pattern (before/after).
        /// </summary>
        /// <param name="action">the action to execute within the tracer</param>
        void EndTraceSuccess(Action<IProcessAnalyticsTracer> action);

        /// <summary>
        /// Finish to trace a process and mark the execution as failed (and collects metrics during its execution).
        /// A traced process is stored by categories.
        /// This method is used for tracing but with the asynchronous style pattern (before/after).
        /// </summary>
        /// <param name="action">the action to execute within the tracer</param>
        void EndTraceFailure(Exception e, Action<IProcessAnalyticsTracer> action);
    }
}
