using System;
using System.Diagnostics;
using System.Text;
using Kinetix.Monitoring.Abstractions;
using Microsoft.Extensions.Logging;

namespace KinetixCore.Monitoring
{
    public class ProcessAnalyticsTracer : IProcessAnalyticsTracer, IDisposable
    {
        private ILogger _logger;
        private Boolean? _succeeded; //default no info
        private Exception _causeException; //default no info
        private Action<IAProcess> _consumer;

        private Func<ProcessAnalyticsTracer> parentOptSupplier;
        private AProcessBuilder processBuilder;

        public ProcessAnalyticsTracer(string category, string name, Action<IAProcess> consumer, Func<ProcessAnalyticsTracer> parentOptSupplier, ILoggerFactory loggerFactory)
        {
            Debug.Assert(!String.IsNullOrEmpty(category));
            Debug.Assert(!String.IsNullOrEmpty(name));
            Debug.Assert(consumer != null);
            //---
            _logger = loggerFactory.CreateLogger(category);

            this._consumer = consumer;
            this.parentOptSupplier = parentOptSupplier;

            processBuilder = AProcess.Builder(category, name);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Start " + name);
            }
        }

        public IProcessAnalyticsTracer AddTag(string name, string value)
        {
            processBuilder.AddTag(name, value);
            return this;
        }

        public IProcessAnalyticsTracer IncMeasure(string name, double value)
        {
            processBuilder.IncMeasure(name, value);
            return this;
        }

        public IProcessAnalyticsTracer SetMeasure(string name, double value)
        {
            processBuilder.SetMeasure(name, value);
            return this;
        }


        public void Dispose()
        {
            if (_succeeded.HasValue)
            {
                SetMeasure("success", _succeeded.Value ? 100 : 0);
            }
            if (_causeException != null)
            {
                AddTag("exception", _causeException.GetType().Name);
            }
            AProcess process = processBuilder.Build();

            ProcessAnalyticsTracer parentOpt = parentOptSupplier.Invoke();
            if (parentOpt != null)
            {
                //when the current process is a subProcess, it's finished and must be added to the parent
                parentOpt.processBuilder.AddSubProcess(process);
            }
            else
            {
                //when the current process is the root process, it's finished and must be sent to the connector
                _consumer.Invoke(process);
            }
            LogProcess(process);
        }

        private void LogProcess(AProcess process)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                StringBuilder sb = new StringBuilder()
                        .Append("Finish ")
                        .Append(process.Name)
                        .Append(_succeeded.HasValue ? (_succeeded.Value ? " successfully" : " with error") : "with internal error")
                        .Append(" in ( ")
                        .Append(process.DurationMillis())
                        .Append(" ms)");
                if (process.Measures.Count > 0)
                {
                    sb.Append(" measures:").Append(process.Measures);
                }
                if (process.Tags.Count > 0)
                {
                    sb.Append(" metaData:").Append(process.Tags);
                }
                _logger.LogInformation(sb.ToString());
            }
        }

        public ProcessAnalyticsTracer MarkAsSucceeded()
        {
            //the last mark wins
            //so we prefer to reset causeException
            _causeException = null;
            _succeeded = true;
            return this;
        }

        public ProcessAnalyticsTracer MarkAsFailed(Exception e)
        {
            //We don't check the nullability of e
            //the last mark wins
            //so we prefer to put the flag 'succeeded' to false
            _succeeded = false;
            _causeException = e;
            return this;
        }


    }
}
