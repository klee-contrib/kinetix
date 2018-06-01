using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Kinetix.Monitoring.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DiagnosticAdapter;
using Newtonsoft.Json;

namespace KinetixCore.Monitoring
{
    public class AnalyticsEFCommandListener : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
    {
        private readonly IAnalyticsManager _analyticsManager;
        private static readonly string MS_EF_CORE = "Microsoft.EntityFrameworkCore";
        private static readonly string MODIFIED_ROW = "nbModifiedRow";

        public AnalyticsEFCommandListener(IAnalyticsManager analyticsManager)
        {
            _analyticsManager = analyticsManager;
        }

        public AnalyticsEFCommandListener()
        {
        }


        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="kv">The current notification information.</param>
        public void OnNext(KeyValuePair<string, object> kv)
        {
            var key = kv.Key;
            var val = kv.Value;

            if (key == RelationalEventId.CommandExecuting.Name)
            {
                if (val is CommandEventData commandData)
                {
                    string async = commandData.IsAsync ? "Async" : "";
                    byte[] hash;
                    using (var md5 = MD5.Create())
                    {
                        hash = md5.ComputeHash(Encoding.ASCII.GetBytes(commandData.Command.CommandText));
                    }
                    // We take only the first 8 bytes and we convert it to hexadecimal
                    StringBuilder sbHashId = new StringBuilder();
                    for (int i = 0; i < 8; i++)
                    {
                        sbHashId.Append(hash[i].ToString("X2"));
                    }

                    IDictionary<string, string> dic = new Dictionary<string, string>();

                    foreach (DbParameter param in commandData.Command.Parameters)
                    {
                        dic[param.ParameterName] = param.Value.ToString();
                    }

                    void action(IProcessAnalyticsTracer tracer)
                    {
                        tracer.AddTag("sql_request", commandData.Command.CommandText);
                        tracer.AddTag("sql_params", JsonConvert.SerializeObject(dic));
                    }

                    _analyticsManager.BeginTrace("sql", commandData.ExecuteMethod.ToString() + async + "/" + sbHashId.ToString(), action);
                }
            }
            else if (key == RelationalEventId.CommandExecuted.Name)
            {
                if (val is CommandExecutedEventData commandExecutedData)
                {

                    if (commandExecutedData.IsAsync == false)
                    {
                        if (commandExecutedData.Result.GetType() == typeof(int))
                        {
                            // ExecuteNonQuery
                            _analyticsManager.EndTraceSuccess((tracer) => tracer.SetMeasure(MODIFIED_ROW, (int)commandExecutedData.Result));
                        }
                        else if (commandExecutedData.Result.GetType() == typeof(RelationalDataReader))
                        {
                            // RelationalDataReader
                            RelationalDataReader rdr = (RelationalDataReader)commandExecutedData.Result;
                            _analyticsManager.EndTraceSuccess((tracer) => tracer.SetMeasure(MODIFIED_ROW, (int)rdr.DbDataReader.RecordsAffected));
                        }
                        else
                        {
                            // ExecuteScalar
                            _analyticsManager.EndTraceSuccess((tracer) => tracer.SetMeasure(MODIFIED_ROW, 1));
                        }
                    }
                }
            }
            else if (key == RelationalEventId.CommandError.Name)
            {
                if (val is CommandErrorEventData commandData)
                {
                    _analyticsManager.EndTraceFailure(commandData.Exception, (tracer) => { tracer.AddTag("request", commandData.Command.CommandText); });
                }
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void OnCommandExecuted(object result, bool async)
        {
            var tracer = _analyticsManager.GetCurrentTracer();

            if (async == false)
            {
                if (result.GetType() == typeof(int))
                {
                    // ExecuteNonQuery
                    tracer.SetMeasure(MODIFIED_ROW, (int)result);
                }
                else if (result.GetType() == typeof(RelationalDataReader))
                {
                    // RelationalDataReader
                    RelationalDataReader rdr = (RelationalDataReader)result;

                    tracer.SetMeasure(MODIFIED_ROW, rdr.DbDataReader.RecordsAffected);
                }
                else
                {
                    // ExecuteScalar
                    tracer.SetMeasure(MODIFIED_ROW, 1);
                }
            }

        }

        public void OnCompleted()
        {
            // NOP
        }

        public void OnError(Exception error)
        {
            // NOP
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == MS_EF_CORE)
            {
                value.Subscribe(this);
            }
        }
    }
}
