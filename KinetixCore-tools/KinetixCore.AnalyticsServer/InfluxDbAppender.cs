using System;
using System.Collections.Generic;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using System.Linq;
using InfluxDB.Net.Infrastructure.Influx;

namespace KinetixCore.AnalyticsServer
{
    public class InfluxDbAppender
    {

        private readonly static string TAG_NAME = "name";
	    private readonly static string TAG_LOCATION = "location";

        public async void WriteLogEvent(LogMessage logMessage)
        {

            var _client = new InfluxDb("http://analytica.part.klee.lan.net:8086", "analytica", "kleeklee");
            List<Database> databases = await _client.ShowDatabasesAsync();

            if (!databases.Select(db => db.Name).Any(dbName => dbName.Equals(logMessage.AppName)))
            {
                await _client.CreateDatabaseAsync(logMessage.AppName);
            }

            if (logMessage.LogEvent !=null)
            {
                IList<Point> points = ProcessToPoints(logMessage.LogEvent, logMessage.Host);

                InfluxDbApiResponse writeResponse = await _client.WriteAsync(logMessage.AppName, points.ToArray(), "autogen");
            }
        }


        public static IList<Point> ProcessToPoints(AProcess process, string host)
        {
            IList <Point> points = new List<Point>();

            FlatProcess(process, new Stack<string>(), points, host);

            return points;
        }


        private static Point ProcessToPoint(AProcess process, VisitState visitState, string host)
        {
            IDictionary<string, object> countFields = visitState.CountsByCategory.ToDictionary( kv => kv.Key + "_count", kv => (object) kv.Value);
            IDictionary<string, object> durationFields = visitState.DurationsByCategory.ToDictionary(kv => kv.Key + "_duration", kv => (object)kv.Value);

            // we add a inner duration for convinience
            long innerDuration = process.DurationMillis() - process.SubProcesses.Sum(p => p.DurationMillis());

            Dictionary<string, object> tags = new Dictionary<string, object>() {
                { TAG_NAME, ProperString(process.Name) },
                { TAG_LOCATION, host }
            };

            foreach(var kv in process.Tags)
            {
                tags.Add(ProperString(kv.Key), ProperString(kv.Value));
            }

            Dictionary<string, object> fields = new Dictionary<string, object>()
            {
                { "duration", process.DurationMillis() },
                { "subprocesses", process.SubProcesses.Count },
                { "name", process.Name },
                { "inner_duration", innerDuration },
            };

            foreach(var kv in countFields)
            {
                fields.Add(kv.Key, kv.Value);
            }
            foreach (var kv in durationFields)
            {
                fields.Add(kv.Key, kv.Value);
            }

            return new Point()
            {
                Measurement = process.Category,
                Timestamp = new DateTime(process.Start),
                Tags = tags,
                Fields = fields,
            };
        }

        private static string ProperString(string str)
        {
            return str?.Replace("\n", " ");
        }

        private static VisitState FlatProcess(AProcess process, Stack<string> upperCategory, IList<Point> points, string host)
        {
            VisitState visitState = new VisitState(upperCategory);

            foreach (AProcess subProcess in process.SubProcesses)
            {
                visitState.Push(subProcess);
                //on descend => stack.push
                VisitState childVisiteState = FlatProcess(subProcess, upperCategory, points, host);
                visitState.Merge(childVisiteState);
                //on remonte => stack.poll
                visitState.Pop();
            }

            points.Add(ProcessToPoint(process, visitState, host));

            return visitState;
        }

    }
}
