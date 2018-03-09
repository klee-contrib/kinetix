using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KinetixCore.AnalyticsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AnalyticsTcpServer analyticsServer = new AnalyticsTcpServer();
            analyticsServer.Start(4564);
        }

    }
}
