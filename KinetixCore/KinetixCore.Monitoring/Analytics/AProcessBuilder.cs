using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KinetixCore.Monitoring
{
    /// <summary>
    /// Process to hold monitoring data.
    /// </summary>
    public class AProcessBuilder
    {
        private string myCategory;
        private DateTime start;
        private string myName;
        private IDictionary<string, double> measures;
        private IDictionary<string, string> tags;

        private IList<AProcess> subProcesses;

        /// <summary>
        /// Process builder
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        internal AProcessBuilder(string category, string name)
        {
            myCategory = category;
            myName = name;

            measures = new Dictionary<string, double>();
            tags = new Dictionary<string, string>();
            subProcesses = new List<AProcess>();
            start = DateTime.Now;
        }

        public AProcessBuilder IncMeasure(string measureName, double measureValue)
        {
            Debug.Assert(measureName != null, "Measure name is required");
            //---------------------------------------------------------------------
            double? lastmValue = measures[measureName];
            measures[measureName] = lastmValue == null ? measureValue : measureValue + lastmValue.Value;
            return this;
        }

        public AProcessBuilder SetMeasure(string name, double value)
        {
            Debug.Assert(name != null, "measure name is required");
            //---------------------------------------------------------------------
            measures[name] = value;
            return this;
        }

        public AProcessBuilder AddTag(string name, string value)
        {
            Debug.Assert(name != null, "tag name is required");
            Debug.Assert(value != null, "tag value is required");
            //---------------------------------------------------------------------
            tags[name] = value;
            return this;
        }

        public AProcessBuilder AddSubProcess(AProcess subProcess)
        {
            Debug.Assert(subProcess != null, "sub process is required ");
            //---------------------------------------------------------------------
            subProcesses.Add(subProcess);
            return this;
        }

        public AProcess Build()
        {
            DateTime end = DateTime.Now;
            return new AProcess(
                    myCategory,
                    myName,
                    start,
                    end,
                    measures,
                    tags,
                    subProcesses);
        }
    }
}
