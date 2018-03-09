using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.AnalyticsServer
{
    internal class VisitState
    {
        public IDictionary<string, int?> CountsByCategory { get; } = new Dictionary<string, int?>();
        public IDictionary<string, long?> DurationsByCategory { get; } = new Dictionary<string, long?>();
        private readonly Stack<String> Stack;


        public VisitState(Stack<string> upperCategory)
        {
            Stack = upperCategory;
        }

        public void Push(AProcess process)
        {
            IncDurations(process.Category, process.DurationMillis());
            IncCounts(process.Category, 1);
            Stack.Push(process.Category);
        }

        public void Merge(VisitState visitState)
        {
            foreach(var entry in visitState.DurationsByCategory)
            {
                IncDurations(entry.Key, entry.Value.Value);
            }

            foreach (var entry in visitState.CountsByCategory)
            {
                IncCounts(entry.Key, entry.Value.Value);
            }

        }

        public void Pop()
        {
            Stack.Pop();
        }

        private void IncDurations(string category, long duration)
        {
            if (!Stack.Contains(category))
            {
                long? existing;
                DurationsByCategory.TryGetValue(category, out existing);
                if (existing == null)
                {
                    DurationsByCategory[category] = duration;
                }
                else
                {
                    DurationsByCategory[category] = existing + duration;
                }
            }
        }


        private void IncCounts(String category, int count)
        {
            int? existing;
            CountsByCategory.TryGetValue(category, out existing);

            if (existing == null)
            {
                CountsByCategory[category] = count;
            }
            else
            {
                CountsByCategory[category] = existing + count;
            }
        }


    }
}
