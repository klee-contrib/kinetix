using InfluxDB.Net.Models;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using InfluxDB.Net;

namespace KinetixCore.AnalyticsServer
{
    public class AnalyticsTcpServer
    {
        public void Start(int port) {

            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);

            try {
                tcpListener.Start();

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        public void HandleClient(object obj)
        {
            InfluxDbAppender appender = new InfluxDbAppender();

            TcpClient client = (TcpClient) obj;

            using (StreamReader sReader = new StreamReader(client.GetStream(), Encoding.UTF8))
            {
                while (!sReader.EndOfStream)
                {
                    string sData = sReader.ReadLine();

                    var loggingEvent = JObject.Parse(sData);
                    var message = (string) loggingEvent["Message"];

                    if (!String.IsNullOrEmpty(message))
                    {
                        try
                        {
                            var process = JObject.Parse(message);
                            Console.WriteLine("> " + process);
                            LogMessage logMessage = JsonConvert.DeserializeObject<LogMessage>(message);
                            
                            appender.WriteLogEvent(logMessage);
                        }
                        catch (JsonReaderException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
        }
    }
}
